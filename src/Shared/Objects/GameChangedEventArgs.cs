namespace HoU.GuildBot.Shared.Objects;

public class GameChangedEventArgs
{
    public AvailableGame Game { get; }

    public GameModification GameModification { get; }

    public GameChangedEventArgs(AvailableGame game,
                                GameModification gameModification)
    {
        Game = game ?? throw new ArgumentNullException(nameof(game));
        if (gameModification == GameModification.Undefined)
            throw new ArgumentOutOfRangeException(nameof(gameModification));
        GameModification = gameModification;
    }
}