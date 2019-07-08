namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using DAL;
    using StrongTypes;

    public interface INonMemberRoleProvider
    {
        IDiscordAccess DiscordAccess { set; }

        Task SetNonMemberRole(DiscordChannelID channelID, DiscordUserID userID, string emoji);

        Task RevokeNonMemberRole(DiscordChannelID channelID, DiscordUserID userID, string emoji);
    }
}