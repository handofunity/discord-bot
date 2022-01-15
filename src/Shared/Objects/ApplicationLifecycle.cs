using System.Threading;

namespace HoU.GuildBot.Shared.Objects;

public static class ApplicationLifecycle
{
    private static readonly CancellationTokenSource _cts;

    public static CancellationToken CancellationToken => _cts.Token;

    static ApplicationLifecycle()
    {
        _cts = new CancellationTokenSource();
    }

    /// <summary>
    /// Ends the application lifecycle and initializes the shutdown of the application.
    /// </summary>
    public static void End()
    {
        _cts.Cancel();
    }
}