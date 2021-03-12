using System;
using System.Threading.Tasks;
using global::Discord.Commands;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.Attributes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;

namespace HoU.GuildBot.DAL.Discord.Modules
{
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

        [Command("aocclassesimage")]
        [CommandCategory(CommandCategory.GameAshesOfCreation, 1)]
        [Name("Get an image about the aoc classes")]
        [Summary("Creates and posts an image that shows the current amount of classes.")]
        [Alias("aoc classes image", "aocclassesstatistic", "aoc classes statistic", "classes", "class")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public Task GetAocClassesImage()
        {
            var channel = Context.Channel;

            // The rest of this runs in a fire & forget task to not block the gateway
            _ = Task.Run(async () =>
            {
                try
                {
                    using var state = channel.EnterTypingState();
                    await using var imageStream = _imageProvider.CreateAocClassDistributionImage();
                    await channel.SendFileAsync(imageStream, "currentAocClasses.png");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to provide AoC class statistic image to channel {channel.Name}.");
                }
            }).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        [Command("aocclasslist")]
        [CommandCategory(CommandCategory.GameAshesOfCreation, 3)]
        [Name("Get an image of all aoc classes")]
        [Summary("Creates and posts an image with all 64 AoC classes.")]
        [Alias("aoc class list", "aoc classlist", "classlist", "class list")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public Task GetAocClassListImage()
        {
            var channel = Context.Channel;

            // The rest of this runs in a fire & forget task to not block the gateway
            _ = Task.Run(async () =>
            {
                try
                {
                    using var state = channel.EnterTypingState();
                    await using var imageStream = _imageProvider.LoadClassListImage();
                    await channel.SendFileAsync(imageStream, "aocClassList.jpg");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to provide aoc class list image to channel {channel.Name}.");
                }
            }).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        [Command("aocplaystylesimage")]
        [CommandCategory(CommandCategory.GameAshesOfCreation, 4)]
        [Name("Get an image about the aoc play styles")]
        [Summary("Creates and posts an image that shows the current amount of play styles.")]
        [Alias("aoc playstyles image", "aocplaystylesstatistic", "aoc playstyles statistic", "playstyles", "playstyle")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public Task GetAocPlayStylesImage()
        {
            var channel = Context.Channel;

            // The rest of this runs in a fire & forget task to not block the gateway
            _ = Task.Run(async () =>
            {
                try
                {
                    using var state = channel.EnterTypingState();
                    await using var imageStream = _imageProvider.CreateAocPlayStyleDistributionImage();
                    await channel.SendFileAsync(imageStream, "currentAocPlayStyles.png");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to provide aoc play styles statistic image to channel {channel.Name}.");
                }
            }).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        [Command("aocracesimage")]
        [CommandCategory(CommandCategory.GameAshesOfCreation, 5)]
        [Name("Get an image about the aoc races")]
        [Summary("Creates and posts an image that shows the current amount of races.")]
        [Alias("aoc races image", "aocracesstatistic", "aoc races statistic", "races", "race")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public Task GetAocRacesImage()
        {
            var channel = Context.Channel;

            // The rest of this runs in a fire & forget task to not block the gateway
            _ = Task.Run(async () =>
            {
                try
                {
                    using var state = channel.EnterTypingState();
                    await using var imageStream = _imageProvider.CreateAocRaceDistributionImage();
                    await channel.SendFileAsync(imageStream, "currentAocRaces.png");
                }
                catch (Exception e)
                {
                    _logger.LogError(e, $"Failed to provide aoc roles statistic image to channel {channel.Name}.");
                }
            }).ConfigureAwait(false);

            return Task.CompletedTask;
        }

        #endregion
    }
}