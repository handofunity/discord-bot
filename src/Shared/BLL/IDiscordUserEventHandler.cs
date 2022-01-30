﻿using System;
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

    /// <summary>
    /// Handles the action a user created on a message component.
    /// </summary>
    /// <param name="userId">The Id of the user who triggered the action.</param>
    /// <param name="customId">The custom Id of the component the user interacted with.</param>
    /// <param name="availableOptions">All available options for the <paramref name="customId"/>.
    /// Should be <b>null</b> for a button.</param>
    /// <param name="selectedValues">The selected values in the given action component.</param>
    /// <returns>Any success or error message that can be forwarded as response.</returns>
    Task<string?> HandleMessageComponentExecutedAsync(DiscordUserId userId,
                                                      string customId,
                                                      IReadOnlyCollection<string>? availableOptions,
                                                      IReadOnlyCollection<string> selectedValues);
}