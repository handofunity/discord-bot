﻿namespace HoU.GuildBot.Shared.Objects
{
    public class AppSettings
    {
        public string BotToken { get; set; }

        public ulong HandOfUnityGuildId { get; set; }

        public ulong LoggingChannelId { get; set; }

        public ulong PromotionAnnouncementChannelId { get; set; }

        public string ConnectionString { get; set; }
    }
}