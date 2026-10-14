using Mattermost.Models.Groups;
using System.Text.Json;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class GroupModelTests
    {
        [Test]
        public void Group_PreservesNullableCountsOpaqueSourceAndExactTimes()
        {
            const string json = "{\"id\":\"group\",\"name\":null,\"display_name\":\"Display\",\"description\":\"Description\",\"source\":\"plugin_example\",\"remote_id\":null,\"create_at\":9007199254740993,\"update_at\":9007199254740994,\"delete_at\":0,\"has_syncables\":true,\"member_count\":0,\"allow_reference\":false,\"channel_member_count\":null,\"channel_member_timezones_count\":2,\"member_ids\":[\"one\",\"two\"]}";
            Group group = JsonSerializer.Deserialize<Group>(json)!;
            Assert.That(group.Name, Is.Null);
            Assert.That(group.Source, Is.EqualTo("plugin_example"));
            Assert.That(group.RemoteId, Is.Null);
            Assert.That(group.CreatedAt, Is.EqualTo(9007199254740993L));
            Assert.That(group.UpdatedAt, Is.EqualTo(9007199254740994L));
            Assert.That(group.HasSyncables, Is.True);
            Assert.That(group.MemberCount, Is.EqualTo(0));
            Assert.That(group.ChannelMemberCount, Is.Null);
            Assert.That(group.ChannelMemberTimezonesCount, Is.EqualTo(2));
            Assert.That(group.AllowReference, Is.False);
            Assert.That(group.MemberIds, Is.EqualTo(new[] { "one", "two" }));
            using JsonDocument encoded = JsonDocument.Parse(JsonSerializer.Serialize(group));
            Assert.That(encoded.RootElement.GetProperty("create_at").GetInt64(), Is.EqualTo(group.CreatedAt));
            Assert.That(encoded.RootElement.GetProperty("member_count").GetInt32(), Is.EqualTo(0));
            Assert.That(encoded.RootElement.TryGetProperty("channel_member_count", out _), Is.False);
        }

        [TestCase("Team", null)]
        [TestCase("Channel", "channel")]
        public void Syncable_PreservesTeamAndChannelShapes(string type, string? channel)
        {
            string json = "{\"group_id\":\"group\",\"team_id\":\"team\",\"type\":\"" + type
                + "\",\"team_display_name\":\"Team\",\"team_type\":\"I\",\"auto_add\":false,\"scheme_admin\":true,"
                + "\"create_at\":9007199254740993,\"update_at\":9007199254740994,\"delete_at\":0"
                + (channel is null ? "}" : ",\"channel_id\":\"channel\",\"channel_display_name\":\"Channel\",\"channel_type\":\"P\"}");
            GroupSyncable link = JsonSerializer.Deserialize<GroupSyncable>(json)!;
            Assert.That(link.GroupId, Is.EqualTo("group"));
            Assert.That(link.TeamId, Is.EqualTo("team"));
            Assert.That(link.ChannelId, Is.EqualTo(channel));
            Assert.That(link.Type, Is.EqualTo(type));
            Assert.That(link.AutoAdd, Is.False);
            Assert.That(link.SchemeAdmin, Is.True);
            Assert.That(link.CreatedAt, Is.EqualTo(9007199254740993L));
            Assert.That(link.UpdatedAt, Is.EqualTo(9007199254740994L));
            using JsonDocument encoded = JsonDocument.Parse(JsonSerializer.Serialize(link));
            Assert.That(encoded.RootElement.GetProperty("team_id").GetString(), Is.EqualTo("team"));
            Assert.That(encoded.RootElement.TryGetProperty("channel_id", out _), Is.EqualTo(channel is not null));
        }

        [Test]
        public void MemberProfilesAndMembershipRows_AreDistinct()
        {
            GroupMemberList profiles = JsonSerializer.Deserialize<GroupMemberList>("{\"members\":[{\"id\":\"user\",\"username\":\"account\"}],\"total_member_count\":7}")!;
            Assert.That(profiles.TotalMemberCount, Is.EqualTo(7));
            Assert.That(profiles.Members![0].Username, Is.EqualTo("account"));
            GroupMember row = JsonSerializer.Deserialize<GroupMember>("{\"group_id\":\"group\",\"user_id\":\"user\",\"create_at\":9007199254740993,\"delete_at\":0}")!;
            Assert.That(row.GroupId, Is.EqualTo("group"));
            Assert.That(row.UserId, Is.EqualTo("user"));
            Assert.That(row.CreatedAt, Is.EqualTo(9007199254740993L));
            Assert.That(JsonSerializer.Deserialize<GroupMemberList>("{\"members\":null,\"total_member_count\":0}")!.Members, Is.Null);
        }

        [Test]
        public void Associations_PreserveMapShapeTotalsAndFalseAdministratorFlag()
        {
            GroupAssociationList associations = JsonSerializer.Deserialize<GroupAssociationList>("{\"groups\":[{\"id\":\"group\",\"scheme_admin\":false}],\"total_group_count\":11}")!;
            Assert.That(associations.TotalGroupCount, Is.EqualTo(11));
            Assert.That(associations.Groups![0].SchemeAdmin, Is.False);
            TeamChannelGroups channels = JsonSerializer.Deserialize<TeamChannelGroups>("{\"groups\":{\"channel\":[{\"id\":\"group\",\"scheme_admin\":true}]}}")!;
            Assert.That(channels.Groups!["channel"][0].Id, Is.EqualTo("group"));
            Assert.That(channels.Groups["channel"][0].SchemeAdmin, Is.True);
            using JsonDocument encoded = JsonDocument.Parse(JsonSerializer.Serialize(associations));
            Assert.That(encoded.RootElement.GetProperty("groups")[0].GetProperty("scheme_admin").GetBoolean(), Is.False);
        }

        [Test]
        public void Stats_PreserveInt64Count()
        {
            GroupStats stats = JsonSerializer.Deserialize<GroupStats>("{\"group_id\":\"group\",\"total_member_count\":9007199254740993}")!;
            Assert.That(stats.TotalMemberCount, Is.EqualTo(9007199254740993L));
        }
    }
}
