using System;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IDiscordUserEventHandler
    {
        IDiscordAccess DiscordAccess { set; }

        void HandleJoined(DiscordUserID userID, Role roles);

        void HandleLeft(DiscordUserID userID,
                        string username,
                        ushort discriminatorValue,
                        DateTimeOffset? joinedAt,
                        string[] roles);

        UserRolesChangedResult HandleRolesChanged(DiscordUserID userID, Role oldRoles, Role newRoles);

        Task HandleStatusChanged(DiscordUserID userID, bool wasOnline, bool isOnline);

        Task HandleReactionAdded(DiscordChannelID channelID, DiscordUserID userID, ulong messageID, EmojiDefinition emote);

        Task HandleReactionRemoved(DiscordChannelID channelID, DiscordUserID userID, ulong messageID, EmojiDefinition emote);
    }
}