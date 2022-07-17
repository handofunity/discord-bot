namespace HoU.GuildBot.Shared.Objects;

public record TextMessage(string Content,
                          Dictionary<string,Dictionary<string, string>?> CustomIdsAndOptions);