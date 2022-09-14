namespace HoU.GuildBot.BLL;

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

    private async Task<(User User, bool IsNew)> GetUserInternal(DiscordUserId userID)
    {
        try
        {
            await _semaphore.WaitAsync();

            if (!_isInitialized)
                throw new InvalidOperationException("Store is not initialized.");

            var storedUser = _store.SingleOrDefault(m => m.DiscordUserId == userID);
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
            var databaseUser = await _databaseAccess.GetOrAddUserAsync(userID);
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

    async Task IUserStore.Initialize((DiscordUserId UserId, Role Roles, string CurrentRoles, DateTime JoinedDate)[] guildUsersCurrentlyOnServer)
    {
        if (_isInitialized)
            return;
        try
        {
            await _semaphore.WaitAsync();

            _logger.LogInformation("Initializing user store for {UsersOnServer} users on the server...", guildUsersCurrentlyOnServer.Length);
            _logger.LogInformation("Ensuring all users exist in the database...");
            await _databaseAccess.EnsureUsersExistAsync(guildUsersCurrentlyOnServer.Select(m => m.UserId));

            _logger.LogInformation("Loading users from database...");
            var usersInDatabase = await _databaseAccess.GetAllUsers();
            _logger.LogInformation("Loaded {UsersInDatabase} users from the database", usersInDatabase.Length);

            _logger.LogInformation("Intersecting users on server with users in database to apply Discord information for memory-cache...");
            var userInfosToPersistToDatabase = new List<User>();
            var existingUsers = usersInDatabase.Join(guildUsersCurrentlyOnServer,
                                                     user => user.DiscordUserId,
                                                     tuple => tuple.UserId,
                                                     (user, tuple) =>
                                                     {
                                                         var persist = true;
                                                         user.Roles = tuple.Roles;
                                                         if (user.CurrentRoles != tuple.CurrentRoles)
                                                         {
                                                             user.CurrentRoles = tuple.CurrentRoles;
                                                             persist = true;
                                                         }
                                                         if (tuple.JoinedDate != user.JoinedDate && tuple.JoinedDate != User.DefaultJoinedDate)
                                                         {
                                                             user.JoinedDate = tuple.JoinedDate;
                                                             persist = true;
                                                         }

                                                         if (persist)
                                                             userInfosToPersistToDatabase.Add(user);

                                                         return user;
                                                     }).ToArray();

            foreach (var user in existingUsers)
                _store.Add(user);

            if (userInfosToPersistToDatabase.Any())
            {
                _logger.LogInformation("Persisting user information about {Amount} users to the database ...", userInfosToPersistToDatabase.Count);
                await _databaseAccess.UpdateUserInformationAsync(userInfosToPersistToDatabase);
            }

            _isInitialized = true;
            _logger.LogInformation("User store initialized with {Users} users in memory-cache", _store.Count);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    bool IUserStore.TryGetUser(DiscordUserId userID, out User? user)
    {
        try
        {
            _semaphore.Wait();

            if (!_isInitialized)
                throw new InvalidOperationException("Store is not initialized.");

            user = _store.SingleOrDefault(m => m.DiscordUserId == userID);
            var success = user != null;
            if (!success)
                _logger.LogWarning("Failed to get user {DiscordUserId} from the user store", userID);
            return success;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    bool IUserStore.TryGetUser(InternalUserId userID, out User? user)
    {
        try
        {
            _semaphore.Wait();

            if (!_isInitialized)
                throw new InvalidOperationException("Store is not initialized.");

            user = _store.SingleOrDefault(m => m.InternalUserId == userID);
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

    async Task IUserStore.AddUserIfNewAsync(DiscordUserId userID, Role roles)
    {
        var result = await GetUserInternal(userID);
        result.User.Roles = roles;
    }

    async Task IUserStore.RemoveUser(DiscordUserId userID)
    {
        try
        {
            await _semaphore.WaitAsync();

            if (!_isInitialized)
                throw new InvalidOperationException("Store is not initialized.");

            _store.RemoveAll(m => m.DiscordUserId == userID);
        }
        finally
        {
            _semaphore.Release();
        }
    }

    #endregion
}