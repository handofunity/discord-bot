namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using Objects;

    public interface IMessageProvider
    {
        Task<EmbedData> ListAllMessages();

        Task<string> GetMessage(string name);

        Task<(bool Success, string Response)> SetMessage(string name, string content);
    }
}