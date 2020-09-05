using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.BLL
{
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
                await _semaphore.WaitAsync();

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
                await _semaphore.WaitAsync();
                // If the user wasn't found, make sure it exists in the database and load it
                var databaseUser = await _databaseAccess.GetOrAddUser(userID);
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

        async Task IUserStore.Initialize((DiscordUserID UserId, Role Roles)[] guildUsersCurrentlyOnServer)
        {
            if (_isInitialized)
                return;
            try
            {
                await _semaphore.WaitAsync();

                _logger.LogInformation("Initializing user store for {UsersOnServer} users on the server...", guildUsersCurrentlyOnServer.Length);
                _logger.LogInformation("Ensuring all users exist in the database...");
                await _databaseAccess.EnsureUsersExist(guildUsersCurrentlyOnServer.Select(m => m.UserId));

                _logger.LogInformation("Loading users from database...");
                var usersInDatabase = await _databaseAccess.GetAllUsers();
                _logger.LogInformation("Loaded {UsersInDatabase} from the database.", usersInDatabase.Length);

                _logger.LogInformation("Intersecting users on server with users in database to apply Discord information for memory-cache...");
                var existingUsers = usersInDatabase.Join(guildUsersCurrentlyOnServer,
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
                _logger.LogInformation($"User store initialized with {_store.Count} users in memory-cache.");
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
            var result = await GetUserInternal(userID);
            result.User.Roles = roles;
        }

        async Task IUserStore.RemoveUser(DiscordUserID userID)
        {
            try
            {
                await _semaphore.WaitAsync();

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