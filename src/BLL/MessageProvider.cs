using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using JetBrains.Annotations;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL
{
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

        async Task<(string Name, string Description, string Content)[]> IMessageProvider.ListAllMessages()
        {
            return await _databaseAccess.GetAllMessages().ConfigureAwait(false);
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

        async Task<(bool Success, string Message)> IMessageProvider.SetMessage(string name, string content)
        {
            if (content == null)
                return (false, "Content cannot be <null>.");
            if (content.Length > 2000)
                return (false, "Content length cannot be longer than 2000 characters.");
            var setSuccessfully = await _databaseAccess.SetMessageContent(name, content).ConfigureAwait(false);
            if (setSuccessfully)
            {
                _cache.AddOrUpdate(name, content, (key, currentValue) => content);
                MessageChanged?.Invoke(this, new MessageChangedEventArgs(name));
            }
            return setSuccessfully
                       ? (true, "Message updated successfully.")
                       : (false, $"Failed to update message: message with name '{name}' doesn't exist");
        }

        #endregion
    }
}