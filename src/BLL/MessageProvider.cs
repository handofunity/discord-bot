namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;

    [UsedImplicitly]
    public class MessageProvider : IMessageProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IDatabaseAccess _databaseAccess;
        private readonly ConcurrentDictionary<string, string> _cache;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public MessageProvider(IDatabaseAccess databaseAccess)
        {
            _databaseAccess = databaseAccess;
            _cache = new ConcurrentDictionary<string, string>();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IMessageProvider Members

        public event EventHandler<MessageChangedEventArgs> MessageChanged;

        async Task<EmbedData> IMessageProvider.ListAllMessages()
        {
            var messages = await _databaseAccess.GetAllMessages().ConfigureAwait(false);
            var formatedMessages = messages.Select(m => $"**{m.Name}**: _{m.Description}_{Environment.NewLine}" +
                                                        $"**Current content:**{Environment.NewLine}{m.Content}");
            return new EmbedData
                       {
                           Title = "All messages",
                           Color = Colors.LightGreen,
                           Description = string.Join(Environment.NewLine + Environment.NewLine, formatedMessages)
                       };
        }

        async Task<string> IMessageProvider.GetMessage(string name)
        {
            if (_cache.TryGetValue(name, out var cachedContent))
                return cachedContent;
            var dbContent = await _databaseAccess.GetMessageContent(name).ConfigureAwait(false);
            if (dbContent == null)
                throw new ArgumentOutOfRangeException(nameof(name), $"Message with name '{name}' is not defined.");
            var n = _cache.AddOrUpdate(name, dbContent, (key, currentValue) => dbContent);
            return n;
        }

        async Task<(bool Success, string Response)> IMessageProvider.SetMessage(string name, string content)
        {
            _cache.AddOrUpdate(name, content, (key, currentValue) => content);
            var setSuccessfully = await _databaseAccess.SetMessageContent(name, content).ConfigureAwait(false);
            if (setSuccessfully)
                MessageChanged?.Invoke(this, new MessageChangedEventArgs(name));
            return setSuccessfully
                       ? (true, "Message updated successfully.")
                       : (false, $"Failed to update message: message with name '{name}' doesn't exist");
        }

        #endregion
    }
}