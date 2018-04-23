namespace HoU.GuildBot.DAL.Modules
{
    using System.Threading.Tasks;
    using Discord.Commands;
    using JetBrains.Annotations;
    using Shared.BLL;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class InfoModule : ModuleBaseHoU
    {
        private readonly IBotInformationProvider _botInformationProvider;

        public InfoModule(IBotInformationProvider botInformationProvider)
        {
            _botInformationProvider = botInformationProvider;
        }

        [Command("info")]
        public async Task InfoAsync()
        {
            var data = _botInformationProvider.GetData();
            var embed = BuildEmbedFromData(data);
            await ReplyAsync(string.Empty, false, embed);
        }
    }
}