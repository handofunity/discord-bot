namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Objects;
    using Shared.StrongTypes;

    [UsedImplicitly]
    public class UserStore : IUserStore
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ReaderWriterLockSlim _lock;
        private readonly IDatabaseAccess _databaseAccess;
        private readonly ILogger<IUserStore> _logger;
        private readonly List<User> _store;
        private bool _isInitialized;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public UserStore(IDatabaseAccess databaseAccess,
                         ILogger<IUserStore> logger)
        {
            _databaseAccess = databaseAccess;
            _logger = logger;
            _lock = new ReaderWriterLockSlim();
            _store = new List<User>();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IUserStore Members

        bool IUserStore.IsInitialized => _isInitialized;

        async Task IUserStore.Initialize()
        {
            if (_isInitialized)
                return;
            try
            {
                _logger.LogInformation("Initializing user store...");
                _lock.EnterWriteLock();
                var users = await _databaseAccess.GetAllUsers().ConfigureAwait(false);
                foreach (var user in users)
                    _store.Add(user);
                _isInitialized = true;
                _logger.LogInformation("User store initialized.");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        async Task<User> IUserStore.GetUser(DiscordUserID userID)
        {
            if (_store.Count == 0)
                throw new InvalidOperationException("Store is not initialized.");

            try
            {
                _lock.EnterReadLock();
                var storedUser = _store.SingleOrDefault(m => m.DiscordUserID == userID);
                if (storedUser != null)
                    return storedUser;
            }
            finally
            {
                _lock.ExitReadLock();
            }

            try
            {
                _lock.EnterWriteLock();
                // If the user wasn't found, make sure it exists in the database and load it
                var databaseUser = await _databaseAccess.GetOrAddUser(userID).ConfigureAwait(false);
                _store.Add(databaseUser.User);
                return databaseUser.User;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        User IUserStore.GetUser(InternalUserID userID)
        {
            if (_store.Count == 0)
                throw new InvalidOperationException("Store is not initialized.");

            try
            {
                _lock.EnterReadLock();
                var user = _store.SingleOrDefault(m => m.InternalUserID == userID);
                if (user == null)
                    throw new ArgumentOutOfRangeException($"No user could be found for the user ID {userID}.");
                return user;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        #endregion
    }
}