namespace HoU.GuildBot.Shared.Objects
{
    public class AppSettings
    {
        /// <summary>
        /// Gets or sets the bot token.
        /// </summary>
        public string BotToken { get; set; }

        /// <summary>
        /// Gets or sets the Discord ID of the "Hand of Unity" guild.
        /// </summary>
        public ulong HandOfUnityGuildId { get; set; }

        /// <summary>
        /// Gets or sets the Discord ID of the channel used for logging.
        /// </summary>
        public ulong LoggingChannelId { get; set; }

        /// <summary>
        /// Gets or sets the Discord ID of the channel used for promotion announcements.
        /// </summary>
        public ulong PromotionAnnouncementChannelId { get; set; }

        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        /// <remarks>Should either be a IP/TCP connection, or a named connection. For IP/TCP connections, see example.</remarks>
        /// <example>IPv4: "Server=169.100.10.154\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
        /// IPv6: "Server=fe80::2011:f831:9281:1ffb%23\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"
        /// IPv6: "Server=fe80::2011:f831:9281:1ffb\\MSSQLSERVER,1433;Database=hou-guild;User Id=hou-guildbot;Password=PASSWORD;"</example>
        public string ConnectionString { get; set; }
    }
}