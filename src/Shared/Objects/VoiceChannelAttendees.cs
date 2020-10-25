using System.Collections.Generic;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Objects
{
    public class VoiceChannelAttendees
    {
        public string DiscordVoiceChannelId { get; }

        public List<DiscordUserID> DiscordUserIds { get; }

        public VoiceChannelAttendees(string discordVoiceChannelId,
                                     List<DiscordUserID> discordUserIds)
        {
            DiscordVoiceChannelId = discordVoiceChannelId;
            DiscordUserIds = discordUserIds;
        }
    }
}