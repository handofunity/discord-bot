namespace HoU.GuildBot.Shared.BLL
{
    using System.Collections.Generic;
    using Enums;

    public interface IGuildUserRegistry
    {
        void AddGuildUsers(IEnumerable<(ulong UserId, Role Roles)> guildUsers);
        void RemoveGuildUsers(IEnumerable<ulong> userIds);
        void AddGuildUser(ulong userId, Role roles);
        void RemoveGuildUser(ulong userId);
        void UpdateGuildUser(ulong userId, Role roles);
        Role GetGuildUserRoles(ulong userId);
    }
}