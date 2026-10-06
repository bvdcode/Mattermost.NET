using Mattermost.Enums;
using Mattermost.Models.Channels;

namespace Mattermost.Tests
{
    [TestFixture]
    internal class ChannelTypeTests
    {
        [TestCase("O", ChannelType.Public)]
        [TestCase("P", ChannelType.Private)]
        [TestCase("D", ChannelType.Direct)]
        [TestCase("G", ChannelType.Group)]
        public void ChannelType_RoundTrips(string wireValue, ChannelType channelType)
        {
            Channel channel = new Channel { Type = wireValue };
            Assert.That(channel.ChannelType, Is.EqualTo(channelType));

            channel = new Channel { ChannelType = channelType };
            Assert.That(channel.Type, Is.EqualTo(wireValue));
        }

        [Test]
        public void UnknownChannelType_IsRejected()
        {
            Channel channel = new Channel { Type = "unknown" };

            Assert.Throws<ArgumentOutOfRangeException>(() => _ = channel.ChannelType);
            Assert.Throws<ArgumentOutOfRangeException>(() => channel.ChannelType = (ChannelType)int.MaxValue);
        }
    }
}
