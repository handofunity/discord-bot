using System;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.Shared.BLL
{
    public interface IMessageProvider
    {
        event EventHandler<MessageChangedEventArgs> MessageChanged;

        Task<(string Name, string Description, string Content)[]> ListAllMessages();

        Task<string> GetMessage(string name);

        Task<(bool Success, string Message)> SetMessage(string name, string content);
    }
}