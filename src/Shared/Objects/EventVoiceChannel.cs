using System.Diagnostics;
using Newtonsoft.Json;

namespace HoU.GuildBot.Shared.Objects
{
    [DebuggerDisplay(nameof(DisplayName))]
    public class EventVoiceChannel
    {
        [JsonProperty]
        public byte GroupNumber { get; }

        [JsonIgnore]
        public string DisplayName { get; }

        [JsonIgnore]
        public byte MaxUsersInChannel { get; }

        [JsonIgnore]
        public ulong DiscordVoiceChannelIdValue { get; set; }

        [JsonProperty]
        public string DiscordVoiceChannelId => DiscordVoiceChannelIdValue.ToString();

        public EventVoiceChannel(int appointmentId)
        {
            GroupNumber = 0;
            DisplayName = $"UNITS {appointmentId}: General";
            MaxUsersInChannel = 0;
        }

        public EventVoiceChannel(int appointmentId,
                                 byte groupNumber,
                                 byte maxUsersInChannel)
        {
            GroupNumber = groupNumber;
            DisplayName = $"UNITS {appointmentId}: Group {groupNumber}";
            MaxUsersInChannel = maxUsersInChannel;
        }
    }
}