using System.Text.RegularExpressions;
using System.Threading.Tasks;
using global::Discord.Commands;
using global::Discord.WebSocket;
using JetBrains.Annotations;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.Attributes;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.DAL.Discord.Modules
{
    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class VoiceModule : ModuleBaseHoU
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IVoiceChannelManager _voiceChannelManager;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public VoiceModule(IVoiceChannelManager voiceChannelManager)
        {
            _voiceChannelManager = voiceChannelManager;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Commands

        [Command("voice")]
        [CommandCategory(CommandCategory.Voice, 1)]
        [Name("Creates a voice channel")]
        [Summary("Creates a voice channel for a specific numbers of users, and deletes the channel after everyone left the channel.")]
        [Remarks("Syntax: _voice \"CHANNEL-NAME\" USER-LIMIT_ e.g.: _voice \"My channel\" 5_ to create a voice channel called \"My channel\" with a maximum of 5 users.\r\n" +
                 "Channel name must be unique and not empty. Only letters, digits and spaces are allowed.\r\n" +
                 "User limit must be equal to two or greater.")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.AnyGuildMember)]
        public async Task CreateVoiceChannelAsync([Remainder] string remainder)
        {
            var regex = new Regex(@"^""(?<name>[\w ]+)"" (?<maxUsers>\d+)$");
            var match = regex.Match(remainder);
            if (!match.Success)
            {
                await ReplyAsync("Couldn't parse command parameter from message content. Please use the help function to see the correct command syntax.")
                   .ConfigureAwait(false);
                return;
            }

            var error = await _voiceChannelManager.CreateVoiceChannel(match.Groups["name"].Value, int.Parse(match.Groups["maxUsers"].Value))
                                                  .ConfigureAwait(false);
            if (error == null)
            {
                await ReplyAsync($"{Context.User.Mention}: Voice channel \"{match.Groups["name"].Value}\" has been created successfully.").ConfigureAwait(false);
            }
            else
            {
                await ReplyAsync($"{Context.User.Mention}: Failed to create voice channel. Reason: `{error}`").ConfigureAwait(false);
            }
        }

        [Command("mute")]
        [CommandCategory(CommandCategory.Voice, 2)]
        [Name("Mute all users")]
        [Summary("Mutes all players in the current channel with permissions below the bot.")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Leader | Role.Officer | Role.Coordinator)]
        public async Task MuteUsersAsync()
        {
            var error = await _voiceChannelManager.TryToMuteUsers((DiscordUserID) Context.User.Id,
                                                                  Context.User.Mention)
                                                  .ConfigureAwait(false);
            if (error != null)
                await ReplyAsync(error).ConfigureAwait(false);
        }

        [Command("unmute")]
        [CommandCategory(CommandCategory.Voice, 3)]
        [Name("Unmute all users")]
        [Summary("Unmutes all players in the current channel with permissions below the bot.")]
        [RequireContext(ContextType.Guild)]
        [ResponseContext(ResponseType.AlwaysSameChannel)]
        [RolePrecondition(Role.Leader | Role.Officer | Role.Coordinator)]
        public async Task UnmuteUsersAsync()
        {
            var error = await _voiceChannelManager.TryToUnMuteUsers((DiscordUserID) Context.User.Id,
                                                                    Context.User.Mention)
                                                  .ConfigureAwait(false);
            if (error != null)
                await ReplyAsync(error).ConfigureAwait(false);
        }

        #endregion
    }
}