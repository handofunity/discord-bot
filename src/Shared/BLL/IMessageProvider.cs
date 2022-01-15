using System.Threading.Tasks;

namespace HoU.GuildBot.Shared.BLL;

public interface IMessageProvider
{
    Task<string> GetMessage(string name);
}