namespace HoU.GuildBot.DAL.Modules
{
    using System;
    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;
    using JetBrains.Annotations;
    using Shared.Objects;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class InfoModule : ModuleBase<SocketCommandContext>
    {
        private readonly RuntimeInformation _runtimeInformation;

        public InfoModule(RuntimeInformation runtimeInformation)
        {
            _runtimeInformation = runtimeInformation;
        }

        [Command("info")]
        public async Task InfoAsync()
        {
            var embedBuilder = new EmbedBuilder()
                               .WithTitle("Bot information")
                               .WithColor(Color.Orange)
                               .AddField(builder => builder.WithName("Environment")
                                                           .WithValue(_runtimeInformation.Environment))
                               .AddField(builder => builder.WithName("Server time")
                                                           .WithValue(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss \"UTC\"zzz")))
                               .AddField(builder => builder.WithName("Start-time")
                                                           .WithValue(_runtimeInformation.StartTime + " UTC"))
                               .AddField(builder => builder.WithName("UP-time")
                                                           .WithValue((DateTime.Now.ToUniversalTime() - _runtimeInformation.StartTime).ToString(@"hh\:mm\:ss")));
            var embed = embedBuilder.Build();
            await ReplyAsync(string.Empty, false, embed);
        }
    }
}