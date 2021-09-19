using System;
using System.Threading.Tasks;
using global::Discord.Commands;
using JetBrains.Annotations;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.Attributes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.StrongTypes;
using Microsoft.Extensions.Logging;

namespace HoU.GuildBot.DAL.Discord.Modules
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class HelpModule : ModuleBaseHoU
    {
        private readonly IHelpProvider _helpProvider;
        private readonly ILogger _logger;

        public HelpModule(IHelpProvider helpProvider,
                          ILogger logger)
        {
            _helpProvider = helpProvider;
            _logger = logger;
        }

        [Command("help")]
        [CommandCategory(CommandCategory.Help, 1)]
        [Name("Get command help")]
        [Summary("Provides help for commands.")]
        [Remarks("If no further arguments are provided, this command will list all available commands.")]
        [Alias("?")]
        [RequireContext(ContextType.DM | ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysDirect)]
        [RolePrecondition(Role.AnyGuildMember)]
        public Task HelpAsync([Remainder] string helpRequest = null)
        {
            _logger.LogDebug("Received \"help\" command request ...");

            (string Message, Shared.Objects.EmbedData EmbedData)[] data;
            try
            {
                data = _helpProvider.GetHelp((DiscordUserID)Context.User.Id, helpRequest);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, $"Failed to get help data from {nameof(IHelpProvider)}.");
                return Task.CompletedTask;
            }

            _ = Task.Run(async () => await data.PerformBulkOperation(async t =>
            {
                try
                {
                    var embed = t.EmbedData?.ToEmbed();
                    await ReplyPrivateAsync(t.Message, embed).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Failed to send help data via DM.");
                }
            }).ConfigureAwait(false));

            return Task.CompletedTask;
        }
    }
}