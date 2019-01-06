namespace HoU.GuildBot.DAL.Discord.Modules
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using global::Discord.Commands;
    using JetBrains.Annotations;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.Enums;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class InfoModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IBotInformationProvider _botInformationProvider;
        private readonly ITimeInformationProvider _timeInformationProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public InfoModule(IBotInformationProvider botInformationProvider,
                          ITimeInformationProvider timeInformationProvider)
        {
            _botInformationProvider = botInformationProvider;
            _timeInformationProvider = timeInformationProvider;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("info")]
        [CommandCategory(CommandCategory.Administration, 1)]
        [Name("Get bot information")]
        [Summary("Gets information about the current bot instance.")]
        [Alias("information")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer | Role.Leader)]
        public async Task InfoAsync()
        {
            var data = _botInformationProvider.GetData();
            var embed = data.ToEmbed();
            await ReplyAsync(string.Empty, false, embed).ConfigureAwait(false);
        }

        [Command("time")]
        [CommandCategory(CommandCategory.MemberInformation, 5)]
        [Name("Get guild times")]
        [Summary("Gets a list of guild time zones.")]
        [Alias("times", "timezone", "timezones", "guildtime", "guild time", "guildtimes", "guild times")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public async Task TimeAsync()
        {
            var tz = _timeInformationProvider.GetCurrentTimeFormattedForConfiguredTimezones();
            var markdownBuilder = new StringBuilder()
                                 .AppendLine("```md")
                                 .AppendLine("Current Guild Times")
                                 .AppendLine("===================");
            foreach (var s in tz)
                markdownBuilder.AppendLine(s);
            markdownBuilder.AppendLine("```");
            await ReplyAsync(markdownBuilder.ToString()).ConfigureAwait(false);
        }

        #endregion
    }
}