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
    using Shared.Enums;
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
        #region Private Methods

        private async Task<(User User, bool IsNew)> GetUserInternal(DiscordUserID userID)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Store is not initialized.");

            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                var storedUser = _store.SingleOrDefault(m => m.DiscordUserID == userID);
                if (storedUser != null)
                    return (storedUser, false);
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
                // Only add the user to the store if it is really new.
                // We might end up in this block to add the user to the database,
                // but it might have been added to the store due to another block before entering the semaphore here.
                if (databaseUser.IsNew)
                    _store.Add(databaseUser.User);
                return databaseUser;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IUserStore Members

        bool IUserStore.IsInitialized => _isInitialized;

        async Task IUserStore.Initialize((DiscordUserID UserId, Role Roles)[] guildUsers)
        {
            if (_isInitialized)
                return;
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);
                _logger.LogInformation("Initializing user store...");
                _logger.LogInformation("Loading users from database...");
                var users = await _databaseAccess.GetAllUsers().ConfigureAwait(false);
                _logger.LogInformation("Applying Discord information to users...");
                var existingUsers = users.Join(guildUsers,
                    user => user.DiscordUserID,
                    tuple => tuple.UserId,
                    (user, tuple) =>
                    {
                        user.Roles = tuple.Roles;
                        return user;
                    }).ToArray();
                foreach (var user in existingUsers)
                    _store.Add(user);
                await _databaseAccess.EnsureUsersExist(existingUsers.Select(m => m.DiscordUserID)).ConfigureAwait(false);
                _isInitialized = true;
                _logger.LogInformation($"User store initialized with {_store.Count} users.");
            }
            finally
            {
                _semaphore.Release();
            }
        }

        User IUserStore.GetUser(DiscordUserID userID)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Store is not initialized.");

            try
            {
                _semaphore.Wait();
                var user = _store.SingleOrDefault(m => m.DiscordUserID == userID);
                if (user == null)
                    throw new KeyNotFoundException($"No user could be found for the user ID {userID}.");
                return user;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        User IUserStore.GetUser(InternalUserID userID)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Store is not initialized.");

            try
            {
                _semaphore.Wait();
                var user = _store.SingleOrDefault(m => m.InternalUserID == userID);
                if (user == null)
                    throw new KeyNotFoundException($"No user could be found for the user ID {userID}.");
                return user;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        User[] IUserStore.GetUsers(Predicate<User> predicate)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Store is not initialized.");

            try
            {
                _semaphore.Wait();
                return _store.Where(m => predicate(m)).ToArray();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        async Task<(User User, bool IsNew)> IUserStore.GetOrAddUser(DiscordUserID userID, Role roles)
        {
            var result = await GetUserInternal(userID).ConfigureAwait(false);
            result.User.Roles = roles;
            return result;
        }

        void IUserStore.RemoveUser(DiscordUserID userID)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("Store is not initialized.");

            try
            {
                _semaphore.Wait();
                _store.RemoveAll(m => m.DiscordUserID == userID);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        #endregion
    }
}