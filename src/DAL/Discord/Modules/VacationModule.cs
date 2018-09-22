namespace HoU.GuildBot.DAL.Discord.Modules
{
    using System;
    using System.Globalization;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using global::Discord.Commands;
    using global::Discord.WebSocket;
    using JetBrains.Annotations;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.Enums;
    using Shared.StrongTypes;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class VacationModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IVacationProvider _vacationProvider;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public VacationModule(IVacationProvider vacationProvider)
        {
            _vacationProvider = vacationProvider;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("add vacation")]
        [Name("Add vacation entry")]
        [Summary("Adds a vacation entry.")]
        [Remarks("Syntax: _add vacation yyyy-mm-dd to yyyy-mm-dd optional note_ e.g.: _add vacation 2018-05-19 to 2018-05-20 weekend trip_.\r\n" +
                 "The note and end date are optional, therefore _add vacation yyyy-mm-dd_, _add vacation yyyy-mm-dd to yyyy-mm-dd_ and _add vacation yyyy-mm-dd optional note_ are also valid.\r\n" +
                 "Vacations can be added up to 12 months into the future.")]
        [Alias("addvacation")]
        [RequireContext(ContextType.DM)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public async Task AddVacationAsync([Remainder] string messageContent)
        {
            // Parse message content
            var regex = new Regex(@"^(?<start>\d{4}-\d{2}-\d{2})( (to|until|up to|till|-) (?<end>\d{4}-\d{2}-\d{2}))? (?<note>.*)?$");
            var match = regex.Match(messageContent);
            if (!match.Success)
            {
                await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                return;
            }

            var startParsed = DateTime.TryParseExact(match.Groups["start"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start);
            if (!startParsed)
            {
                await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                return;
            }
            DateTime end;
            if (match.Groups["end"].Success)
            {
                var endParsed = DateTime.TryParseExact(match.Groups["end"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out end);
                if (!endParsed)
                {
                    await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                    return;
                }
            }
            else
            {
                end = start;
            }
            var note = match.Groups["note"].Success
                           ? match.Groups["note"].Value
                           : null;

            var response = await _vacationProvider.AddVacation((DiscordUserID)Context.User.Id, start, end, note).ConfigureAwait(false);
            await ReplyAsync(response).ConfigureAwait(false);
        }

        [Command("list vacations")]
        [Name("List all vacations.")]
        [Summary("Lists all current and upcoming vacations, or for a specfic date.")]
        [Remarks("Can be called with an optional data parameter to get the vacations for a specific date, e.g.: _list vacations 2018-05-20_ to get a list of everybody who's on vacation on the 20th May 2018.\r\n" +
                 "Can also be called with parameters \"today\" and \"tomorrow\", e.g.: _list vacations today_ or _list vacations tomorrow_")]
        [Alias("listvacations", "vacations")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Leader | Role.Officer)]
        public async Task ListVacationsAsync([Remainder] string messageContent = null)
        {
            if (messageContent == null)
            {
                var response = await _vacationProvider.GetVacations().ConfigureAwait(false);
                await ReplyAsync(response).ConfigureAwait(false);
            }
            else
            {
                switch (messageContent.ToLower())
                {
                    case "today":
                    {
                        var response = await _vacationProvider.GetVacations(DateTime.Today).ConfigureAwait(false);
                        await ReplyAsync(response).ConfigureAwait(false);
                        break;
                    }
                    case "tomorrow":
                    {
                        var response = await _vacationProvider.GetVacations(DateTime.Today.AddDays(1)).ConfigureAwait(false);
                        await ReplyAsync(response).ConfigureAwait(false);
                        break;
                    }
                    default:
                    {
                        var regex = new Regex(@"^(?<date>\d{4}-\d{2}-\d{2})$");
                        var match = regex.Match(messageContent);
                        if (!match.Success
                            || !DateTime.TryParseExact(match.Groups["date"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                        {
                            await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                            return;
                        }
                        var response = await _vacationProvider.GetVacations(date).ConfigureAwait(false);
                        await ReplyAsync(response).ConfigureAwait(false);
                        break;
                    }
                }
            }
        }

        [Command("list vacations")]
        [Name("List user related vacations.")]
        [Summary("Lists all current and upcoming vacations for a specific user.")]
        [Remarks("Syntax: _list vacations @User_")]
        [Alias("listvacations", "vacations")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Leader | Role.Officer)]
        public async Task ListVacationsForUserAsync(SocketGuildUser guildUser)
        {
            var response = await _vacationProvider.GetVacations((DiscordUserID)guildUser.Id).ConfigureAwait(false);
            await ReplyAsync(response).ConfigureAwait(false);
        }

        [Command("delete vacation")]
        [Name("Delete vacation entry")]
        [Summary("Deletes a vacation entry.")]
        [Remarks("Syntax: _delete vacation yyyy-mm-dd to yyyy-mm-dd_ e.g.: _delete vacation 2018-05-19 to 2018-05-20_.\r\n" +
                 "The end date is optional, therefore _delete vacation yyyy-mm-dd_ is also valid.")]
        [Alias("deletevacation")]
        [RequireContext(ContextType.DM)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public async Task DeleteVacationAsync([Remainder] string messageContent)
        {
            // Parse message content
            var regex = new Regex(@"^(?<start>\d{4}-\d{2}-\d{2})( (to|until|up to|till|-) (?<end>\d{4}-\d{2}-\d{2}))?$");
            var match = regex.Match(messageContent);
            if (!match.Success)
            {
                await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                return;
            }

            var startParsed = DateTime.TryParseExact(match.Groups["start"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var start);
            if (!startParsed)
            {
                await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                return;
            }
            DateTime end;
            if (match.Groups["end"].Success)
            {
                var endParsed = DateTime.TryParseExact(match.Groups["end"].Value, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out end);
                if (!endParsed)
                {
                    await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.").ConfigureAwait(false);
                    return;
                }
            }
            else
            {
                end = start;
            }

            var response = await _vacationProvider.DeleteVacation((DiscordUserID)Context.User.Id, start, end).ConfigureAwait(false);
            await ReplyAsync(response).ConfigureAwait(false);
        }

        [Command("list my vacations")]
        [Name("Lists your vacations.")]
        [Summary("Lists all current and upcoming vacations for you.")]
        [Alias("listmyvacations", "myvacations", "my vacations")]
        [RequireContext(ContextType.DM)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public async Task ListUserVacationsAsync()
        {
            var response = await _vacationProvider.GetVacations((DiscordUserID)Context.User.Id).ConfigureAwait(false);
            await ReplyAsync(response).ConfigureAwait(false);
        }

        #endregion
    }
}