namespace HoU.GuildBot.Shared.Extensions
{
    using System;
    using Enums;

    public static class EnumExtensions
    {
        public static string GetDisplayName(this CommandCategory commandCategory)
        {
            switch (commandCategory)
            {
                case CommandCategory.Undefined:
                    return nameof(CommandCategory.Undefined);
                case CommandCategory.Administration:
                    return nameof(CommandCategory.Administration);
                case CommandCategory.Help:
                    return nameof(CommandCategory.Help);
                case CommandCategory.MemberInformation:
                    return "Member Information";
                case CommandCategory.MemberManagement:
                    return "Member Management";
                case CommandCategory.GameAshesOfCreation:
                    return "Ashes of Creation";
                case CommandCategory.Voice:
                    return nameof(CommandCategory.Voice);
                default:
                    throw new ArgumentOutOfRangeException(nameof(commandCategory), commandCategory, null);
            }
        }

        public static string GetDisplayName(this RequestType requestType)
        {
            switch (requestType)
            {
                case RequestType.Undefined:
                    return nameof(ResponseType.Undefined);
                case RequestType.GuildChannel | RequestType.DirectMessage:
                    return "guild channels and direct messages";
                case RequestType.GuildChannel:
                    return "guild channels";
                case RequestType.DirectMessage:
                    return "direct messages";
                default:
                    throw new ArgumentOutOfRangeException(nameof(requestType), requestType, null);
            }
        }

        public static string GetDisplayName(this ResponseType responseType)
        {
            switch (responseType)
            {
                case ResponseType.Undefined:
                    return nameof(ResponseType.Undefined);
                case ResponseType.AlwaysDirect:
                    return "a direct message channel";
                case ResponseType.AlwaysSameChannel:
                    return "always the same channel";
                case ResponseType.MultipleChannels:
                    return "multiple channels";
                default:
                    throw new ArgumentOutOfRangeException(nameof(responseType), responseType, null);
            }
        }

        public static string GetDisplayName(this Role role)
        {
            switch (role)
            {
                case Role.AnyGuildMember:
                    return "any guild member";
                default:
                    return role.ToString();
            }
        }
    }
}