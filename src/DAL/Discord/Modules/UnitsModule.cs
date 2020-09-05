using System;
using System.Threading;
using System.Threading.Tasks;
using global::Discord;
using global::Discord.Commands;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.Attributes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.DAL.Discord.Modules
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class UnitsModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IImageProvider _imageProvider;
        private readonly ILogger<UnitsModule> _logger;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public UnitsModule(IImageProvider imageProvider,
                           ILogger<UnitsModule> logger)
        {
            _imageProvider = imageProvider;
            _logger = logger;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("profile")]
        [CommandCategory(CommandCategory.GameAshesOfCreation, 2)]
        [Name("Get the profile card")]
        [Summary("Gets the UNITS profile card.")]
        [Alias("profilecard", "profile card", "card")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Developer)] // TODO: Change to any guild member
        public Task GetProfileCardAsync()
        {
            try
            {
                ThreadPool.QueueUserWorkItem(async state =>
                {
                    var c = (SocketCommandContext)state;

                    try
                    {
                        using (c.Channel.EnterTypingState())
                        {
                            using (var imageStream = await _imageProvider.CreateProfileImage((DiscordUserID) c.User.Id,
                                                                                             c.User.GetAvatarUrl(ImageFormat.Png)))
                            {
                                await c.Channel.SendFileAsync(imageStream, $"{c.User.Discriminator}_{DateTime.UtcNow:yyyyMMddHHmmss}.png").ConfigureAwait(false);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, $"Failed to provide UNITS profile card image to channel {c.Channel.Name}.");
                    }

                }, Context);
            }
            catch (NotSupportedException e)
            {
                _logger.LogError(e, $"Failed to provide UNITS profile card image to channel {Context.Channel.Name}, error while queueing user work item.");
            }

            return Task.CompletedTask;
        }

        #endregion
    }
}