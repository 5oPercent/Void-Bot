using Void.Bot.Message.Constants.EventAction;
using Void.Bot.Message.Dto.EventAction;
using Void.Bot.Message.Events.EventAction;

namespace Void.Bot.Core.Services.EventAction;

public static class EventActionConditionEvaluator
{
    public static string? Validate(
        BuiltInEventDefinitionDto eventDefinition,
        IReadOnlyCollection<EventActionConditionDto> conditions)
    {
        foreach (var condition in conditions)
        {
            if (condition == null)
            {
                return "Event condition cannot be null.";
            }

            var fields = eventDefinition.Fields
                .Where(x => string.Equals(x.Name, condition.Field, StringComparison.OrdinalIgnoreCase))
                .ToList();
            if (fields.Count != 1)
            {
                return $"Event condition field '{condition.Field}' must have exactly one definition, but found {fields.Count}.";
            }

            var field = fields[0];
            if (!field.Operators.Contains(condition.Operator, StringComparer.OrdinalIgnoreCase))
            {
                return $"Event condition field '{field.Name}' does not support operator '{condition.Operator}'.";
            }

            var values = (condition.Values ?? []).Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            var requiresSingleValue =
                string.Equals(condition.Operator, EventActionConditionOperators.Equal, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(condition.Operator, EventActionConditionOperators.NotEqual, StringComparison.OrdinalIgnoreCase);
            if (values.Count == 0 || requiresSingleValue && values.Count != 1)
            {
                return requiresSingleValue
                    ? $"Event condition operator '{condition.Operator}' requires exactly one value."
                    : $"Event condition operator '{condition.Operator}' requires at least one value.";
            }

            if (string.Equals(field.ValueType, "Guid", StringComparison.OrdinalIgnoreCase) &&
                values.Any(x => !Guid.TryParse(x, out _)))
            {
                return $"Event condition field '{field.Name}' requires GUID values.";
            }
        }

        return null;
    }

    public static bool Matches(
        IBuiltInEventDefinitionProvider eventDefinitionProvider,
        IBuiltInEvent builtInEvent,
        IReadOnlyCollection<EventActionConditionDto> conditions)
    {
        if (conditions.Count == 0)
        {
            return true;
        }

        var eventValues = eventDefinitionProvider.GetFieldValues(builtInEvent);
        foreach (var condition in conditions)
        {
            var field = eventDefinitionProvider.Definition.Fields.Single(x =>
                string.Equals(x.Name, condition.Field, StringComparison.OrdinalIgnoreCase));
            var actualValues = eventValues.First(x =>
                string.Equals(x.Key, field.Name, StringComparison.OrdinalIgnoreCase)).Value;
            var containsMatch = actualValues.Any(actual =>
                condition.Values.Any(expected => ValuesEqual(actual, expected, field.ValueType)));

            var matches = condition.Operator.ToUpperInvariant() switch
            {
                "EQUAL" => containsMatch,
                "NOTEQUAL" => !containsMatch,
                "IN" => containsMatch,
                "NOTIN" => !containsMatch,
                _ => false
            };
            if (!matches)
            {
                return false;
            }
        }

        return true;
    }

    private static bool ValuesEqual(string actual, string expected, string valueType)
    {
        if (string.Equals(valueType, "Guid", StringComparison.OrdinalIgnoreCase) &&
            Guid.TryParse(actual, out var actualGuid) &&
            Guid.TryParse(expected, out var expectedGuid))
        {
            return actualGuid == expectedGuid;
        }

        return string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase);
    }
}
