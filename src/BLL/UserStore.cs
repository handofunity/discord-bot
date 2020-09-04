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
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);

                if (!_isInitialized)
                    throw new InvalidOperationException("Store is not initialized.");

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
                _logger.LogInformation("Ensuring all users exist on the database...");
                await _databaseAccess.EnsureUsersExist(guildUsers.Select(m => m.UserId)).ConfigureAwait(false);
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
                _isInitialized = true;
                _logger.LogInformation($"User store initialized with {_store.Count} users.");
                if (_store.Count < guildUsers.Length)
                    _logger.LogWarning("Loaded less users into the store ({LoadedCount}) than given ({GivenCount}).",
                                       _store.Count,
                                       guildUsers.Length);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        bool IUserStore.TryGetUser(DiscordUserID userID, out User user)
        {
            try
            {
                _semaphore.Wait();

                if (!_isInitialized)
                    throw new InvalidOperationException("Store is not initialized.");

                user = _store.SingleOrDefault(m => m.DiscordUserID == userID);
                var success = user != null;
                if (!success)
                    _logger.LogWarning("Failed to get user {DiscordUserId} from the user store.", userID);
                return success;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        bool IUserStore.TryGetUser(InternalUserID userID, out User user)
        {
            try
            {
                _semaphore.Wait();

                if (!_isInitialized)
                    throw new InvalidOperationException("Store is not initialized.");

                user = _store.SingleOrDefault(m => m.InternalUserID == userID);
                return user != null;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        User[] IUserStore.GetUsers(Predicate<User> predicate)
        {
            try
            {
                _semaphore.Wait();

                if (!_isInitialized)
                    throw new InvalidOperationException("Store is not initialized.");

                return _store.Where(m => predicate(m)).ToArray();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        async Task IUserStore.AddUserIfNewAsync(DiscordUserID userID, Role roles)
        {
            var result = await GetUserInternal(userID).ConfigureAwait(false);
            result.User.Roles = roles;
        }

        async Task IUserStore.RemoveUser(DiscordUserID userID)
        {
            try
            {
                await _semaphore.WaitAsync().ConfigureAwait(false);

                if (!_isInitialized)
                    throw new InvalidOperationException("Store is not initialized.");

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