using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Objects;

public class SpamLimit
{
    /// <summary>
    /// Gets or sets if the <see cref="SoftCap"/> and <see cref="HardCap"/> are restricted to a certain channel, or if the values apply globally.
    /// </summary>
    public DiscordChannelId RestrictToChannelId { get; set; }

    /// <summary>
    /// Gets or sets the soft cap for this spam limit.
    /// </summary>
    /// <remarks>Hitting the soft cap will result in a warning/notification.</remarks>
    public int SoftCap { get; set; }

    /// <summary>
    /// Gets or sets the hard cap for this spam limit.
    /// </summary>
    /// <remarks>Hitting the hard cap will result in a kick.</remarks>
    public int HardCap { get; set; }
}