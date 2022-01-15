using JetBrains.Annotations;

namespace HoU.GuildBot.Shared.Objects;

public class BotAuthenticationResponse
{
    public string? Token { get; [UsedImplicitly] set; }
}