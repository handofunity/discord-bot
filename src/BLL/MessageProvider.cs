namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class MessageProvider : IMessageProvider
{
    private readonly IDatabaseAccess _databaseAccess;
    
    public MessageProvider(IDatabaseAccess databaseAccess)
    {
        _databaseAccess = databaseAccess;
    }

    async Task<string> IMessageProvider.GetMessageAsync(string name)
    {
        var messageContent = await _databaseAccess.GetMessageContentAsync(name);
        if (messageContent is null)
            throw new ArgumentOutOfRangeException(nameof(name), $"Message with name '{name}' is not defined.");
        return messageContent;
    }
}