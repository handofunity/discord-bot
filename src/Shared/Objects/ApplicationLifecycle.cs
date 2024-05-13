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
    /// <param name="source">Name of the calling source, for logging purposes.</param>
    public static void End(string source)
    {
        Console.Out.WriteLine($"[ApplicationLifecycle] End triggered from '{source}'.");
        _cts.Cancel();
    }
}