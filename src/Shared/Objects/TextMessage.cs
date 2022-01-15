using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Objects;

public record TextMessage(DiscordChannelId ChannelId,
                          DiscordMessageId MessageId,
                          string Content,
                          string[] CustomIds);