"""Execute the SQL against an explicitly labelled, network-isolated disposable MySQL.

Run: python tests/migrations/test_current_era_configuration.py --container NAME
No application compilation, business database connection or external Python package is used.
"""
import argparse
import copy
import json
from pathlib import Path
import re
import subprocess
import sys
import unittest
import uuid


ROOT = Path(__file__).resolve().parents[2]
SCRIPTS = ROOT / "src/Void.Bot.Core/Dbup/Scripts"
WORKSPACE = "20000000-0000-0000-0000-000000000002"
CATALOG = "20000000-0000-0000-0000-000000000001"
parser = argparse.ArgumentParser()
parser.add_argument("--container", required=True)
ARGS, TEST_ARGS = parser.parse_known_args()


def docker(*args, source=None):
    return subprocess.run(["docker", *args], input=source, capture_output=True)


def literal(value):
    # Hex literals avoid dependence on SQL backslash escaping and connection character sets.
    return "CONVERT(0x" + value.encode("utf-8").hex() + " USING utf8mb4)"


def document(value):
    return literal(json.dumps(value, ensure_ascii=False, separators=(",", ":")))


def relations(label="原有"):
    return {
        "Species": [{"Id": "human", "Name": label + "人类"}, {"Id": "construct", "Name": "构造体"}],
        "Factions": [{"Id": "players", "Name": "玩家"}, {"Id": "hostile", "Name": label + "敌对"}],
        "SpeciesRules": [{"ObserverId": "human", "TargetId": "construct", "Relation": 3, "Bidirectional": False}],
        "FactionRules": [{"ObserverId": "players", "TargetId": "hostile", "Relation": 4, "Bidirectional": True}],
    }


def configuration():
    return {
        "SchemaVersion": 3,
        "Systems": [{"Id": "shared.key", "Name": "晶格", "PresentationKey": "lattice.presentation", "Description": "保留技能池",
                     "SpecialSkills": [], "PrimarySkills": ["shared.key"], "SecondarySkills": [], "TacticalSkills": []}],
        "Skills": [{"Id": "shared.key", "Name": "技能\"\\\n测试", "Description": "原技能", "Variants": [
            {"Slot": 1, "MaximumCharges": 3, "CooldownSeconds": 2.75, "CarrierKind": 0,
             "Effects": [{"EffectId": "shared.key", "Condition": None}]}]}],
        "Buffs": [{"Id": "shared.key", "Name": "效果🔥", "Description": "保留效果数值", "Kind": 0,
                   "DurationSeconds": 0, "IntervalSeconds": -1,
                   "Effects": [{"Kind": 1, "Amount": 9007199254740993, "Stat": 0, "Operation": 0}]}],
        "Enemies": [], "CommonTacticalSkills": ["shared.key"], "DisabledSkills": ["shared.key"],
        "CombatRelations": relations(),
    }


class CurrentConfigurationMigration(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        result = docker("inspect", "--format", "{{json .Config.Labels}}|{{.HostConfig.NetworkMode}}|{{json .HostConfig.PortBindings}}", ARGS.container)
        if result.returncode:
            raise RuntimeError(result.stderr.decode(errors="replace"))
        labels, network, ports = result.stdout.decode().strip().split("|")
        if json.loads(labels).get("codex.task") != "era-migration-audit" or network != "none" or ports not in ("null", "{}"):
            raise RuntimeError("Refusing to run: container must have codex.task=era-migration-audit, network=none, and no published ports.")

    def sql(self, source, *, fail=False, database=True):
        command = ["exec", "-i", ARGS.container, "mysql", "-uroot", "--default-character-set=utf8mb4", "--batch", "--raw", "--skip-column-names"]
        if database:
            command.append(self.database)
        result = docker(*command, source=("SET SESSION sql_mode='STRICT_ALL_TABLES,NO_ENGINE_SUBSTITUTION';\n" + source).encode())
        if fail:
            self.assertNotEqual(result.returncode, 0, "Migration should reject the malformed source")
        elif result.returncode:
            self.fail(result.stderr.decode(errors="replace"))
        return result.stdout.decode("utf-8"), result.stderr.decode(errors="replace")

    def setUp(self):
        self.database = "era_audit_" + uuid.uuid4().hex
        self.sql(f"CREATE DATABASE {self.database} CHARACTER SET utf8mb4;", database=False)
        catalog_schema = (SCRIPTS / "Script0026_init_era_skills.sql").read_text(encoding="utf-8").split("CREATE TABLE `era_player_skill_profile`")[0]
        draft_schema = (SCRIPTS / "Script0027_init_era_skill_drafts.sql").read_text(encoding="utf-8").split("CREATE TABLE `era_skill_publication`")[0]
        self.sql(catalog_schema + draft_schema + """
CREATE TABLE era_skill_publication (id INT PRIMARY KEY, payload MEDIUMTEXT);
CREATE TABLE era_skill_publish_receipt (id INT PRIMARY KEY, payload MEDIUMTEXT);
CREATE TABLE menu (id INT PRIMARY KEY, name VARCHAR(128), path VARCHAR(256));
CREATE TABLE permission_menu (menu_id INT NOT NULL);
INSERT INTO menu VALUES (1,'版本管理','/admin/era/version-management'),(2,'种族阵营','/admin/era/combat-relations');
INSERT INTO permission_menu VALUES (1),(2);
""")
        # Representative sentinel rows plus a static write-target check protect every equipment
        # table, independently of the schema/number of columns in a deployed database.
        self.protected = ["era_equipment_configuration", "era_weapon_template", "era_weapon_framework",
                          "era_weapon_framework_capability", "era_weapon_modification", "era_weapon_slot_type",
                          "era_weapon_slot_configuration", "era_weapon_configuration_slot", "era_weapon_configuration_mod_pool",
                          "era_armor_template", "era_armor_modification", "era_armor_slot_type", "era_armor_slot_configuration",
                          "era_armor_configuration_slot", "era_armor_configuration_mod_pool", "era_equipment_instance",
                          "era_reward_event", "era_character_resonance_profile", "era_character_learned_resonance"]
        self.sql("\n".join(f"CREATE TABLE {table} (id INT PRIMARY KEY, payload MEDIUMTEXT); INSERT INTO {table} VALUES (1,{document({'SchemaVersion': 3, '内容': table, '数值': 9007199254740993})});" for table in self.protected))
        self.protected_before = self.protected_rows()
        self.original = configuration()
        self.publish(self.original)

    def tearDown(self):
        self.sql(f"DROP DATABASE {self.database};", database=False)

    def publish(self, value):
        self.sql(f"UPDATE era_skill_configuration SET payload={document(value)},version=17,last_modified_by='11111111-1111-1111-1111-111111111111',updated_at='2026-11-01 12:34:56.123456' WHERE id='{CATALOG}';")

    def draft(self, definitions, settings):
        self.sql(f"UPDATE era_skill_draft_workspace SET initialized=1,revision=23,settings_version=7,settings_payload={document(settings)} WHERE id='{WORKSPACE}';")
        for kind, definition, deleted in definitions:
            self.sql(f"INSERT INTO era_skill_draft_record (id,workspace_id,kind,stable_id,version,payload,is_deleted,was_published) VALUES ('{uuid.uuid4()}','{WORKSPACE}',{kind},{literal(definition['Id'])},11,{document(definition)},{int(deleted)},1);")

    def protected_rows(self):
        return self.sql("\n".join(f"SELECT '{table}',id,payload FROM {table} ORDER BY id;" for table in self.protected))[0]

    def migrate(self, *, fail=False):
        source = "\n".join((SCRIPTS / name).read_text(encoding="utf-8") for name in [
            "Script0048_use_fixed_resonance_configuration.sql", "Script0049_remove_buff_presentation_key.sql",
            "Script0050_current_era_configuration.sql", "Script0051_skill_disabled_state.sql", "Script0052_effect_disabled_state.sql"])
        return self.sql(source, fail=fail)

    def records(self, category):
        rows, _ = self.sql(f"SELECT JSON_OBJECT('key',stable_id,'version',version,'payload',CAST(payload AS JSON)) FROM era_{category}_definition ORDER BY stable_id;")
        return [json.loads(row) for row in rows.splitlines()]

    def migrated_relations(self):
        result, _ = self.sql("SELECT payload FROM era_combat_relation_configuration;")
        return json.loads(result)

    def assert_retired(self):
        tables, _ = self.sql("SHOW TABLES;")
        for table in ["era_skill_configuration", "era_skill_draft_workspace", "era_skill_draft_record", "era_skill_publication", "era_skill_publish_receipt", "era_ability_settings"]:
            self.assertNotIn(table, tables.splitlines())
        self.assertEqual(self.protected_rows(), self.protected_before)
        self.assertEqual(self.sql("SELECT COUNT(*) FROM permission_menu WHERE menu_id=2;")[0].strip(), "1")

    def assert_failed_before_cleanup(self, expected_check):
        _, error = self.migrate(fail=True)
        self.assertIn(expected_check, error)
        tables, _ = self.sql("SHOW TABLES;")
        for table in ["era_skill_configuration", "era_skill_draft_workspace", "era_skill_draft_record"]:
            self.assertIn(table, tables.splitlines())
        self.assertNotIn("era_skill_definition", tables.splitlines())
        current, _ = self.sql("SELECT payload FROM era_skill_configuration;")
        self.assertEqual(json.loads(current), self.original)
        self.assertEqual(self.protected_rows(), self.protected_before)

    def test_published_definitions_relations_flags_and_equipment_are_preserved(self):
        self.migrate()
        for source, category in [("Systems", "lattice"), ("Skills", "skill"), ("Buffs", "effect")]:
            self.assertEqual([row["payload"] for row in self.records(category)], self.original[source])
            self.assertEqual(self.records(category)[0]["version"], 17)
        self.assertEqual(self.migrated_relations(), self.original["CombatRelations"])
        self.assertEqual(self.sql("SELECT is_disabled FROM era_skill_definition;")[0].strip(), "1")
        self.assertEqual(self.sql("SELECT is_disabled FROM era_effect_definition;")[0].strip(), "0")
        self.assert_retired()

    def test_initialized_draft_wins_and_explicit_deletions_stay_deleted(self):
        draft = copy.deepcopy(self.original)
        draft["Systems"][0]["Name"] = "最新晶格"
        draft["Skills"][0]["Name"] = "最新技能"
        draft["Buffs"][0]["Effects"][0]["Amount"] = 73
        items = [(kind, draft[key][0], False) for kind, key in enumerate(["Systems", "Skills", "Buffs"])]
        items.append((1, {"Id": "deleted.skill", "Name": "已删"}, True))
        self.draft(items, {"DisabledSkills": [], "CombatRelations": relations("最新")})
        self.migrate()
        for source, category in [("Systems", "lattice"), ("Skills", "skill"), ("Buffs", "effect")]:
            self.assertEqual([row["payload"] for row in self.records(category)], draft[source])
            self.assertEqual(self.records(category)[0]["version"], 11)
        self.assertEqual(self.migrated_relations(), relations("最新"))
        self.assertEqual(self.sql("SELECT is_disabled FROM era_skill_definition;")[0].strip(), "0")
        self.assert_retired()

    def test_missing_draft_relations_and_flags_fall_back_to_published_values(self):
        self.draft([(kind, self.original[key][0], False) for kind, key in enumerate(["Systems", "Skills", "Buffs"])], {})
        self.migrate()
        self.assertEqual(self.migrated_relations(), self.original["CombatRelations"])
        self.assertEqual(self.sql("SELECT is_disabled FROM era_skill_definition;")[0].strip(), "1")
        self.assert_retired()

    def test_camel_case_catalog_collection_names_keep_the_same_business_payloads(self):
        self.publish({key[0].lower()+key[1:]: value for key, value in self.original.items()})
        self.migrate()
        self.assertEqual(self.records("lattice")[0]["payload"], self.original["Systems"][0])
        self.assertEqual(self.records("effect")[0]["payload"], self.original["Buffs"][0])
        self.assertEqual(self.migrated_relations(), self.original["CombatRelations"])
        self.assert_retired()

    def test_per_definition_disabled_flags_are_moved_to_columns_without_losing_values(self):
        self.original["Skills"][0]["IsDisabled"] = False
        self.original["Buffs"][0]["IsDisabled"] = True
        self.publish(self.original)
        self.migrate()
        self.assertEqual(self.sql("SELECT is_disabled FROM era_skill_definition;")[0].strip(), "0")
        self.assertEqual(self.sql("SELECT is_disabled FROM era_effect_definition;")[0].strip(), "1")
        for source, category in [("Skills", "skill"), ("Buffs", "effect")]:
            expected = {key: value for key, value in self.original[source][0].items() if key != "IsDisabled"}
            self.assertEqual(self.records(category)[0]["payload"], expected)
        self.assert_retired()

    def test_effect_cleanup_preserves_long_payloads_order_and_is_idempotent(self):
        effects = self.original["Buffs"]
        effects[0]["PresentationKey"] = "retired.presentation"
        effects[0]["Description"] = "保留🔥\"\\\n" * 500
        effects.append({"Id": "second.effect", "Name": "第二效果", "presentationKey": "old", "Effects": []})
        self.publish(self.original)
        self.draft([(2, effects[0], False), (2, effects[1], True)], {"CombatRelations": relations()})
        cleanup = (SCRIPTS / "Script0049_remove_buff_presentation_key.sql").read_text(encoding="utf-8")
        self.sql(cleanup)
        expected = [{key: value for key, value in effect.items() if key not in ("PresentationKey", "presentationKey")} for effect in effects]
        saved, _ = self.sql("SELECT payload FROM era_skill_configuration;")
        self.assertEqual(json.loads(saved), {**self.original, "Buffs": expected})
        rows, _ = self.sql("SELECT JSON_OBJECT('payload',CAST(payload AS JSON),'version',version,'deleted',is_deleted) FROM era_skill_draft_record ORDER BY stable_id;")
        migrated = [json.loads(row) for row in rows.splitlines()]
        self.assertEqual([row["payload"] for row in migrated], list(reversed(expected)))
        self.assertEqual([row["version"] for row in migrated], [12, 12])
        self.assertEqual([row["deleted"] for row in migrated], [1, 0])
        self.assertEqual(self.sql("SELECT revision FROM era_skill_draft_workspace;")[0].strip(), "24")
        self.sql(cleanup)
        self.assertEqual(self.sql("SELECT payload FROM era_skill_configuration;")[0], saved)
        self.assertEqual(self.sql("SELECT revision FROM era_skill_draft_workspace;")[0].strip(), "24")
        self.assertEqual(self.sql("SELECT SUM(version) FROM era_skill_draft_record;")[0].strip(), "24")
        self.assertEqual(self.protected_rows(), self.protected_before)

    def test_explicit_empty_draft_settings_do_not_restore_published_choices(self):
        empty = {key: [] for key in relations()}
        self.draft([(kind, self.original[key][0], False) for kind, key in enumerate(["Systems", "Skills", "Buffs"])],
                   {"CombatRelations": empty, "DisabledSkills": []})
        self.migrate()
        self.assertEqual(self.migrated_relations(), empty)
        self.assertEqual(self.sql("SELECT is_disabled FROM era_skill_definition;")[0].strip(), "0")
        self.assert_retired()

    def test_empty_initialized_draft_does_not_resurrect_published_definitions(self):
        self.draft([(kind, self.original[key][0], True) for kind, key in enumerate(["Systems", "Skills", "Buffs"])],
                   {"CombatRelations": relations(), "DisabledSkills": []})
        self.migrate()
        for category in ["lattice", "skill", "effect", "enemy"]:
            self.assertEqual(self.records(category), [])
        self.assert_retired()

    def test_missing_workspace_stops_before_creating_targets_or_deleting_sources(self):
        self.sql("DELETE FROM era_skill_draft_workspace;")
        self.assert_failed_before_cleanup("source_rows")

    def test_invalid_category_shape_stops_before_cleanup(self):
        self.original["Skills"] = {"unexpected": "object"}
        self.publish(self.original)
        self.assert_failed_before_cleanup("array_shape")

    def test_conflicting_array_spellings_stop_before_cleanup(self):
        self.original["skills"] = []
        self.publish(self.original)
        self.assert_failed_before_cleanup("array_case_conflict")

    def test_invalid_disable_flag_stops_before_cleanup(self):
        self.original["Buffs"][0]["IsDisabled"] = "true"
        self.publish(self.original)
        self.assert_failed_before_cleanup("definition_disabled_flag")

    def test_unknown_disabled_skill_stops_before_cleanup(self):
        self.original["DisabledSkills"] = ["missing.skill"]
        self.publish(self.original)
        self.assert_failed_before_cleanup("disabled_skill_references")

    def test_duplicate_definition_keys_stop_before_cleanup(self):
        self.original["Buffs"].append(copy.deepcopy(self.original["Buffs"][0]))
        self.publish(self.original)
        self.assert_failed_before_cleanup("Duplicate entry")

    def test_unknown_draft_category_stops_before_cleanup(self):
        self.draft([(9, {"Id": "unknown"}, False)], {})
        self.assert_failed_before_cleanup("draft_kind")

    def test_copy_verification_retains_sources_if_a_target_payload_changes(self):
        source = (SCRIPTS / "Script0050_current_era_configuration.sql").read_text(encoding="utf-8")
        marker = "-- A duplicate check_name aborts the script instead of silently discarding data.\nINSERT INTO era_current_migration_check (check_name)\nSELECT 'copied_skills'"
        self.assertIn(marker, source)
        source = source.replace(marker, "UPDATE era_skill_definition SET payload=JSON_SET(payload,'$.Name','unexpected change');\n" + marker, 1)
        _, error = self.sql(source, fail=True)
        self.assertIn("copied_skills", error)
        saved, _ = self.sql("SELECT payload FROM era_skill_configuration;")
        self.assertEqual(json.loads(saved), self.original)
        tables, _ = self.sql("SHOW TABLES;")
        for table in ["era_skill_draft_record", "era_skill_draft_workspace", "era_skill_publication", "era_skill_publish_receipt"]:
            self.assertIn(table, tables.splitlines())
        self.assertEqual(self.protected_rows(), self.protected_before)

    def test_migration_write_targets_exclude_all_existing_equipment_and_player_tables(self):
        sql = "\n".join((SCRIPTS / f"Script00{number}_{suffix}.sql").read_text(encoding="utf-8") for number, suffix in [
            (48, "use_fixed_resonance_configuration"), (49, "remove_buff_presentation_key"),
            (50, "current_era_configuration"), (51, "skill_disabled_state"), (52, "effect_disabled_state")])
        sql = re.sub(r"--[^\n]*", "", sql)
        changed = set(re.findall(r"\b(?:UPDATE|INSERT\s+INTO|DELETE\s+FROM|ALTER\s+TABLE|DROP\s+TABLE)\s+`?(\w+)", sql, re.I))
        self.assertFalse(changed.intersection(self.protected), changed.intersection(self.protected))


if __name__ == "__main__":
    unittest.main(argv=[sys.argv[0], *TEST_ARGS], verbosity=2)
