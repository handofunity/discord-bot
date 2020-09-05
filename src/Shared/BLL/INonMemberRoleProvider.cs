using System.Threading.Tasks;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL
{
    public interface INonMemberRoleProvider
    {
        IDiscordAccess DiscordAccess { set; }

        Task SetNonMemberRole(DiscordChannelID channelID, DiscordUserID userID, EmojiDefinition emoji);

        Task RevokeNonMemberRole(DiscordChannelID channelID, DiscordUserID userID, EmojiDefinition emoji);
    }
}