namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using DAL;
    using Objects;
    using StrongTypes;

    public interface INonMemberRoleProvider
    {
        IDiscordAccess DiscordAccess { set; }

        Task SetNonMemberRole(DiscordChannelID channelID, DiscordUserID userID, EmojiDefinition emoji);

        Task RevokeNonMemberRole(DiscordChannelID channelID, DiscordUserID userID, EmojiDefinition emoji);
    }
}