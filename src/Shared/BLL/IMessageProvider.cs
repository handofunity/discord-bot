namespace HoU.GuildBot.Shared.BLL;

public interface IMessageProvider
{
    Task<string> GetMessageAsync(string name);
}