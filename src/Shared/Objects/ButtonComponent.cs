using HoU.GuildBot.Shared.Enums;

namespace HoU.GuildBot.Shared.Objects;

public record ButtonComponent(string CustomId,
                              byte ActionRowNumber,
                              string Label,
                              InteractionButtonStyle Style)
    : ActionComponent(CustomId, ActionRowNumber);