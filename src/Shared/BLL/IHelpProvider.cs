using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IHelpProvider
    {
        (string Message, EmbedData EmbedData)[] GetHelp(DiscordUserID userId, string helpRequest);
    }
}