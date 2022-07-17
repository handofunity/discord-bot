namespace HoU.GuildBot.Shared.Objects;

public class RootSettings
{
    /// <summary>
    /// Gets the primary connection string used to access database objects part of the solution.
    /// </summary>
    /// <remarks>Should either be a IP/TCP connection, or a named connection. For IP/TCP connections, see example.</remarks>
    /// <example>IPv4: "Server=169.100.10.154\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
    /// IPv6: "Server=fe80::2011:f831:9281:1ffb%23\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
    /// IPv6: "Server=fe80::2011:f831:9281:1ffb\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"</example>
    public string ConnectionStringForOwnDatabase { get; }

    /// <summary>
    /// Gets the HangFire connection string used to access database objects part of HangFire.
    /// </summary>
    /// <remarks>Should either be a IP/TCP connection, or a named connection. For IP/TCP connections, see example.</remarks>
    /// <example>IPv4: "Server=169.100.10.154\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
    /// IPv6: "Server=fe80::2011:f831:9281:1ffb%23\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
    /// IPv6: "Server=fe80::2011:f831:9281:1ffb\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"</example>
    public string ConnectionStringForHangFireDatabase { get; }

    /// <summary>
    /// Gets the URL where the Seq instance for logging is located.
    /// </summary>
    public string SeqServerUrl { get; }

    /// <summary>
    /// Gets the API key for the Seq authentication.
    /// </summary>
    public string SeqApiKey { get; }

    /// <summary>
    /// Gets the token of the bot for the Discord authentication.
    /// </summary>
    public string DiscordBotToken { get; }

    /// <summary>
    /// Gets the complete configuration.
    /// </summary>
    public IConfiguration CompleteConfiguration { get; }

    public RootSettings(string connectionStringForOwnDatabase,
                        string connectionStringForHangFireDatabase,
                        string seqServerUrl,
                        string seqApiKey,
                        string discordBotToken,
                        IConfiguration completeConfiguration)
    {
        ConnectionStringForOwnDatabase = connectionStringForOwnDatabase;
        ConnectionStringForHangFireDatabase = connectionStringForHangFireDatabase;
        SeqServerUrl = seqServerUrl;
        SeqApiKey = seqApiKey;
        DiscordBotToken = discordBotToken;
        CompleteConfiguration = completeConfiguration;
    }
}