namespace HoU.GuildBot.Shared.BLL
{
    using System;
    using System.Threading.Tasks;
    using Objects;

    public interface IMessageProvider
    {
        event EventHandler<MessageChangedEventArgs> MessageChanged;

        Task<(string Name, string Description, string Content)[]> ListAllMessages();

        Task<string> GetMessage(string name);

        Task<(bool Success, string Response)> SetMessage(string name, string content);
    }
}