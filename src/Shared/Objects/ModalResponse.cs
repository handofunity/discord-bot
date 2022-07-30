namespace HoU.GuildBot.Shared.Objects;

public record ModalResponse(DiscordUserId UserId,
                            string CustomId,
                            IReadOnlyCollection<ModalResponseItem> Items);