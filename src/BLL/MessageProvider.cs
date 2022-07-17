namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class MessageProvider : IMessageProvider
{
    private readonly IDatabaseAccess _databaseAccess;
    private readonly ConcurrentDictionary<string, string> _cache;
    
    public MessageProvider(IDatabaseAccess databaseAccess)
    {
        _databaseAccess = databaseAccess;
        _cache = new ConcurrentDictionary<string, string>();
    }

    async Task<string> IMessageProvider.GetMessage(string name)
    {
        if (_cache.TryGetValue(name, out var cachedContent))
            return cachedContent;
        var dbContent = await _databaseAccess.GetMessageContentAsync(name);
        if (dbContent == null)
            throw new ArgumentOutOfRangeException(nameof(name), $"Message with name '{name}' is not defined.");
        var n = _cache.AddOrUpdate(name, dbContent, (key, currentValue) => dbContent);
        return n;
    }
}