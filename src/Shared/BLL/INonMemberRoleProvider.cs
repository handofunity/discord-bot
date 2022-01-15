using System.Collections.Generic;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL;

public interface INonMemberRoleProvider
{
    IDiscordAccess DiscordAccess { set; }

    Task<string> ToggleNonMemberRoleAsync(DiscordUserId userId,
                                          string customId,
                                          IReadOnlyCollection<string> values);
}