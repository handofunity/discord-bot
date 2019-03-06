namespace HoU.GuildBot.DAL.Discord.Modules
{
    using System;
    using System.Threading.Tasks;
    using global::Discord.Commands;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Preconditions;
    using Shared.Attributes;
    using Shared.BLL;
    using Shared.Enums;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class StatisticImageRequestModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IImageProvider _imageProvider;
        private readonly ILogger<StatisticImageRequestModule> _logger;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public StatisticImageRequestModule(IImageProvider imageProvider,
                                           ILogger<StatisticImageRequestModule> logger)
        {
            _imageProvider = imageProvider;
            _logger = logger;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("aocrolesimage")]
        [CommandCategory(CommandCategory.GameAshesOfCreation, 1)]
        [Name("Get an image about the aoc roles")]
        [Summary("Creates and posts an image that shows the current amount of roles.")]
        [Alias("aoc roles image", "aocrolesstatistic", "aoc roles statistic", "roles", "role", "classes", "class")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public async Task GetAocRolesImage()
        {
            var channel = Context.Channel;

#pragma warning disable CS4014 // Fire & forget

            // The rest of this runs in a fire & forget task to not block the gateway
            Task.Run(async () =>
            {
                try
                {
                    using (channel.EnterTypingState())
                    {
                        using (var imageStream = _imageProvider.CreateAocRolesImage())
                        {
                            await channel.SendFileAsync(imageStream, "currentAocRoles.png").ConfigureAwait(false);
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to provide aoc roles statistic image to channel {channel.Name}.");
                }
            }).ConfigureAwait(false);

#pragma warning restore CS4014 // Fire & forget
        }

        #endregion
    }
}