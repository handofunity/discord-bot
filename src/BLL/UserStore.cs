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

        private readonly SemaphoreSlim _semaphore;
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
            _semaphore = new SemaphoreSlim(1, 1);
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
                await _semaphore.WaitAsync().ConfigureAwait(false);
                var users = await _databaseAccess.GetAllUsers().ConfigureAwait(false);
                foreach (var user in users)
                    _store.Add(user);
                _isInitialized = true;
                _logger.LogInformation("User store initialized.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        async Task<User> IUserStore.GetUser(DiscordUserID userID)
        {
            if (_store.Count == 0)
                throw new InvalidOperationException("Store is not initialized.");

            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                var storedUser = _store.SingleOrDefault(m => m.DiscordUserID == userID);
                if (storedUser != null)
                    return storedUser;
            }
            finally
            {
                _semaphore.Release();
            }

            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                // If the user wasn't found, make sure it exists in the database and load it
                var databaseUser = await _databaseAccess.GetOrAddUser(userID).ConfigureAwait(false);
                _store.Add(databaseUser.User);
                return databaseUser.User;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        User IUserStore.GetUser(InternalUserID userID)
        {
            if (_store.Count == 0)
                throw new InvalidOperationException("Store is not initialized.");

            try
            {
                _semaphore.Wait();
                var user = _store.SingleOrDefault(m => m.InternalUserID == userID);
                if (user == null)
                    throw new ArgumentOutOfRangeException($"No user could be found for the user ID {userID}.");
                return user;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #endregion
    }
}