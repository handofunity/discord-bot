namespace HoU.GuildBot.Shared.Objects;

public record SelectMenuComponent(string CustomId,
                                  byte ActionRowNumber,
                                  string Placeholder,
                                  IDictionary<string, string> Options,
                                  bool AllowMultiple)
    : ActionComponent(CustomId, ActionRowNumber);