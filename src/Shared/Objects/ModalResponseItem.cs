namespace HoU.GuildBot.Shared.Objects;

public record ModalResponseItem(string CustomId,
                                IReadOnlyCollection<string> Values);