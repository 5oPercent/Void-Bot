using System.Text.Json;
using Shouldly;
using Void.Bot.Message.Dto.Bungie.Destiny2;
using Void.Bot.Message.Enum.Bungie;

namespace Void.Bot.UnitTest.Bungie.Destiny2;

public class Destiny2PostGameCarnageReportResponseFixture
{
    [Fact]
    public void ShouldDeserializePgcrActivityAndPlayerEntry()
    {
        const string json = """
            {
              "Response": {
                "period": "2023-01-19T14:31:20Z",
                "startingPhaseIndex": 0,
                "activityWasStartedFromBeginning": false,
                "activityDetails": {
                  "referenceId": 1262462921,
                  "instanceId": "12280271243"
                },
                "entries": [
                  {
                    "player": {
                      "destinyUserInfo": {
                        "membershipType": 3,
                        "membershipId": "4611686018490148242",
                        "displayName": "RinRKo"
                      }
                    },
                    "characterId": "2305843009511844485",
                    "values": {
                      "completed": {
                        "basic": { "value": 1, "displayValue": "Yes" }
                      },
                      "activityDurationSeconds": {
                        "basic": { "value": 1497, "displayValue": "24m 57s" }
                      },
                      "startSeconds": {
                        "basic": { "value": 2, "displayValue": "0m 2s" }
                      },
                      "timePlayedSeconds": {
                        "basic": { "value": 1495, "displayValue": "24m 55s" }
                      }
                    }
                  }
                ],
                "teams": []
              },
              "ErrorCode": 1,
              "ErrorStatus": "Success",
              "Message": "Ok"
            }
            """;

        var result = JsonSerializer.Deserialize<Destiny2PostGameCarnageReportResponse>(
            json, JsonSerializerOptions.Web);

        result.ShouldNotBeNull();
        result.ErrorCode.ShouldBe(1);
        result.Response.ShouldNotBeNull();
        result.Response.ActivityWasStartedFromBeginning.ShouldBe(false);
        result.Response.ActivityDetails.InstanceId.ShouldBe(12280271243);
        result.Response.Entries.Count.ShouldBe(1);

        var entry = result.Response.Entries[0];
        entry.CharacterId.ShouldBe("2305843009511844485");
        entry.Player.DestinyUserInfo.MembershipType.ShouldBe(BungieMembershipTypeEnum.Steam);
        entry.Player.DestinyUserInfo.MembershipId.ShouldBe("4611686018490148242");
        entry.Values["startSeconds"].Basic!.Value.ShouldBe(2);
        entry.Values["timePlayedSeconds"].Basic!.Value.ShouldBe(1495);
    }
}
