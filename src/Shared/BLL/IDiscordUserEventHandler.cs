using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL;

public interface IDiscordUserEventHandler
{
    IDiscordAccess DiscordAccess { set; }

    void HandleJoined(DiscordUserId userID, Role roles);

    void HandleLeft(DiscordUserId userID,
                    string username,
                    ushort discriminatorValue,
                    DateTimeOffset? joinedAt,
                    string[] roles);

    UserRolesChangedResult HandleRolesChanged(DiscordUserId userID, Role oldRoles, Role newRoles);

    Task HandleStatusChanged(DiscordUserId userID, bool wasOnline, bool isOnline);

    Task<string?> HandleMessageComponentExecutedAsync(DiscordUserId userId,
                                                string customId,
                                                IReadOnlyCollection<string> values);
}