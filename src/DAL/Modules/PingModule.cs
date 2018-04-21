namespace HoU.GuildBot.DAL.Modules
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using Discord.Commands;
    using Discord.WebSocket;
    using JetBrains.Annotations;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    [Group("ping")]
    public class PingModule : ModuleBase<SocketCommandContext>
    {
        [Command]
        public async Task PingAsync()
        {
            await ReplyAsync("Pong!").ConfigureAwait(false);
        }

        [Command("me")]
        public async Task PingMeAsync()
        {
            await ReplyAsync($"Pong! {Context.User.Mention}").ConfigureAwait(false);
        }

        [Command("user at")]
        public async Task PingUserAtAsync(SocketGuildUser mentionedUser, SocketTextChannel textChannel)
        {
            await textChannel.SendMessageAsync($"Pong! {mentionedUser.Mention}").ConfigureAwait(false);
        }

        [Command("permissions")]
        public async Task PingErrorAsync()
        {
            var currentGuildUser = Context.Guild.GetUser(Context.User.Id);
            var leaderRole = Context.Guild.Roles.Single(m => m.Name == "Leader");
            if (leaderRole.Members.Contains(currentGuildUser))
            {
                await ReplyAsync("Pong!").ConfigureAwait(false);
            }
            else
            {
                throw new UnauthorizedAccessException("You're not authorized to use this command.");
            }
        }
    }
}