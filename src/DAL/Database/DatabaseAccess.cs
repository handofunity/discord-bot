using Game = HoU.GuildBot.DAL.Database.Model.Game;
using User = HoU.GuildBot.Shared.Objects.User;

namespace HoU.GuildBot.DAL.Database;

[UsedImplicitly]
public class DatabaseAccess : IDatabaseAccess
{
    private readonly ILogger<DatabaseAccess> _logger;
    private readonly DbContextOptions<HandOfUnityContext> _trackingContextOptions;
    private readonly DbContextOptions<HandOfUnityContext> _noTrackingContextOptions;

    public DatabaseAccess(ILogger<DatabaseAccess> logger,
                          RootSettings rootSettings)
    {
        _logger = logger;
        var builder = new DbContextOptionsBuilder<HandOfUnityContext>();
        builder.UseNpgsql(rootSettings.ConnectionStringForOwnDatabase);
        _trackingContextOptions = builder.Options;
        builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        _noTrackingContextOptions = builder.Options;
    }

    private HandOfUnityContext GetDbContext(bool withChangeTracking = false) =>
        new(withChangeTracking ? _trackingContextOptions : _noTrackingContextOptions);

    private static User Map(Model.User user) =>
        new((InternalUserId)user.UserId,
            (DiscordUserId)user.DiscordUserId)
        {
            JoinedDate = user.UserInfo?.JoinedDate ?? User.DefaultJoinedDate,
            CurrentRoles = user.UserInfo?.CurrentRoles
        };

    async Task<User[]> IDatabaseAccess.GetAllUsers()
    {
        Model.User[] dbObjects;
        await using (var entities = GetDbContext())
        {
            dbObjects = await entities.User
                                      .Include(m => m.UserInfo)
                                      .ToArrayAsync();
        }

        return dbObjects.Select(Map).ToArray();
    }

    async Task IDatabaseAccess.EnsureUsersExistAsync(IEnumerable<DiscordUserId> userIds)
    {
        await using var entities = GetDbContext(true);
        var existingUserIds = await entities.User.Select(m => m.DiscordUserId).ToArrayAsync();
        var missingUserIds = userIds.Except(existingUserIds.Select(m => (DiscordUserId)(ulong)m)).ToArray();

        if (!missingUserIds.Any())
            return;

        _logger.LogInformation("Adding {AmountOfMissingUsers} missing users to the database...", missingUserIds.Length);
        var added = 0;
        foreach (var missingUserId in missingUserIds)
        {
            entities.User.Add(new Model.User
            {
                DiscordUserId = (decimal)missingUserId
            });
            added++;
        }

        await entities.SaveChangesAsync();
        _logger.LogInformation("Added {UsersAdded} missing users to the database.", added);
    }

    async Task<(User User, bool IsNew)> IDatabaseAccess.GetOrAddUserAsync(DiscordUserId userId)
    {
        await using var entities = GetDbContext(true);
        var decUserId = (decimal)userId;
        var dbObject = await entities.User.SingleOrDefaultAsync(m => m.DiscordUserId == decUserId);
        if (dbObject != null)
            return (Map(dbObject), false);

        // Add missing user
        var newUserObject = new Model.User
        {
            DiscordUserId = decUserId
        };
        entities.User.Add(newUserObject);

        await entities.SaveChangesAsync();
        return (Map(newUserObject), true);
    }

    async Task<string?> IDatabaseAccess.GetMessageContentAsync(string name)
    {
        await using var entities = GetDbContext();
        var match = await entities.Message.SingleOrDefaultAsync(m => m.Name == name);
        return match?.Content;
    }

    async Task<bool> IDatabaseAccess.AddVacationAsync(User user,
                                                      DateTime start,
                                                      DateTime end,
                                                      string? note)
    {
        var internalUserId = (int)user.InternalUserId;
        await using var entities = GetDbContext(true);
        // Check if colliding entry exists
        var collisions = await entities.Vacation
                                       .AnyAsync(m => m.UserId == internalUserId
                                                   && DateOnly.FromDateTime(end) >= m.StartDate
                                                   && DateOnly.FromDateTime(start) <= m.EndDate)
            ;
        if (collisions)
            return false;

        // If no colliding entry exists, we can add the entry
        entities.Vacation.Add(new Vacation
        {
            UserId = internalUserId,
            StartDate = DateOnly.FromDateTime(start),
            EndDate = DateOnly.FromDateTime(end),
            Note = string.IsNullOrWhiteSpace(note) ? null : note[..Math.Min(note.Length, 1024)]
        });
        await entities.SaveChangesAsync();
        return true;
    }

    async Task<bool> IDatabaseAccess.DeleteVacationAsync(User user,
                                                         DateTime start,
                                                         DateTime end)
    {
        await using var entities = GetDbContext(true);
        // Find the matching vacation
        var match = await entities.Vacation
                                  .SingleOrDefaultAsync(m => m.UserId == (int)user.InternalUserId
                                                          && m.StartDate == DateOnly.FromDateTime(start)
                                                          && m.EndDate == DateOnly.FromDateTime(end));
        if (match == null)
            return false;

        entities.Vacation.Remove(match);
        await entities.SaveChangesAsync();
        return true;
    }

    async Task IDatabaseAccess.DeletePastVacationsAsync()
    {
        await using var entities = GetDbContext(true);
        // Get past vacations - we don't need to keep those in the database
        var pastVacations = await entities.Vacation.Where(m => m.EndDate < DateOnly.FromDateTime(DateTime.Today)).ToArrayAsync();
        if (!pastVacations.Any())
            return;
        // If any vacations in the past are still in the database, delete them
        entities.Vacation.RemoveRange(pastVacations);
        await entities.SaveChangesAsync();
    }

    async Task IDatabaseAccess.DeleteVacationsAsync(User user)
    {
        await using var entities = GetDbContext(true);
        var vacations = await entities.Vacation.Where(m => m.UserId == (int)user.InternalUserId).ToArrayAsync();
        if (!vacations.Any())
            return;
        // If the user has any vacations in the database, delete them
        entities.Vacation.RemoveRange(vacations);
        await entities.SaveChangesAsync();
    }

    async Task<(DiscordUserId UserId, DateTime Start, DateTime End, string? Note)[]> IDatabaseAccess.GetVacationsAsync()
    {
        await using var entities = GetDbContext();
        var localItems = await entities.Vacation
                                       .Where(m => m.EndDate >= DateOnly.FromDateTime(DateTime.Today))
                                                                        .Join(entities.User,
                                                                              vacation => vacation.UserId,
                                                                              u => u.UserId,
                                                                              (vacation,
                                                                               u) => new { u.DiscordUserId, vacation.StartDate, vacation.EndDate, vacation.Note })
                                                                        .ToArrayAsync()
            ;
        return localItems.Select(m => ((DiscordUserId)m.DiscordUserId,
                                       m.StartDate.ToDateTime(TimeOnly.MinValue),
                                       m.EndDate.ToDateTime(TimeOnly.MinValue),
                                       m.Note))
                         .ToArray();
    }

    async Task<(DiscordUserId UserId, DateTime Start, DateTime End, string? Note)[]> IDatabaseAccess.GetVacationsAsync(User user)
    {
        await using var entities = GetDbContext();
        var localItems = await entities.Vacation
                                       .Where(m => m.EndDate >= DateOnly.FromDateTime(DateTime.Today))
                                       .Join(entities.User,
                                             vacation => vacation.UserId,
                                             u => u.UserId,
                                             (vacation,
                                              u) => new { u.UserId, u.DiscordUserId, vacation.StartDate, vacation.EndDate, vacation.Note })
                                       .Where(m => m.UserId == (int)user.InternalUserId)
                                       .ToArrayAsync()
            ;
        return localItems.Select(m => ((DiscordUserId)m.DiscordUserId,
                                       m.StartDate.ToDateTime(TimeOnly.MinValue),
                                       m.EndDate.ToDateTime(TimeOnly.MinValue),
                                       m.Note))
                         .ToArray();
    }

    async Task<(DiscordUserId UserId, DateTime Start, DateTime End, string? Note)[]> IDatabaseAccess.GetVacationsAsync(DateTime date)
    {
        await using var entities = GetDbContext();
        var localItems = await entities.Vacation
                                       .Where(m => DateOnly.FromDateTime(date) >= m.StartDate
                                                && DateOnly.FromDateTime(date) <= m.EndDate)
                                       .Join(entities.User,
                                             vacation => vacation.UserId,
                                             user => user.UserId,
                                             (vacation,
                                              user) => new { user.DiscordUserId, vacation.StartDate, vacation.EndDate, vacation.Note })
                                       .ToArrayAsync();

        return localItems.Select(m => ((DiscordUserId)m.DiscordUserId,
                                       m.StartDate.ToDateTime(TimeOnly.MinValue),
                                       m.EndDate.ToDateTime(TimeOnly.MinValue),
                                       m.Note))
                         .ToArray();
    }

    async Task<AvailableGame[]> IDatabaseAccess.GetAvailableGamesAsync()
    {
        await using var entities = GetDbContext();
        var localItems = await entities.Game.Include(m => m.GameRole).ToArrayAsync();
        return localItems.Select(m =>
        {
            var g = new AvailableGame
            {
                PrimaryGameDiscordRoleId = (DiscordRoleId)(ulong)m.PrimaryGameDiscordRoleId,
                IncludeInGuildMembersStatistic = m.IncludeInGuildMembersStatistic,
                IncludeInGamesMenu = m.IncludeInGamesMenu,
                GameInterestRoleId = m.GameInterestRoleId == null
                                         ? null
                                         : (DiscordRoleId)(ulong)m.GameInterestRoleId
            };
            g.AvailableRoles.AddRange(m.GameRole.Select(n => new AvailableGameRole
                                                            { DiscordRoleId = (DiscordRoleId)Convert.ToUInt64(n.DiscordRoleId) }));
            return g;
        }).ToArray();
    }

    async Task IDatabaseAccess.UpdateUserInfoLastSeenAsync(User user,
                                                           DateTime lastSeen)
    {
        await using var entities = GetDbContext(true);
        var existingInfo = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserId == (int)user.InternalUserId);
        if (existingInfo is not null)
        {
            existingInfo.LastSeen = lastSeen;
            await entities.SaveChangesAsync();
            return;
        }

        // Create new user info
        entities.UserInfo.Add(new UserInfo { UserId = (int)user.InternalUserId, LastSeen = lastSeen });
        await entities.SaveChangesAsync();
    }

    async Task IDatabaseAccess.UpdateUserInformationAsync(IEnumerable<User> users)
    {
        var localData = users.ToArray();
        var userIds = localData.Select(m => (int)m.InternalUserId).ToArray();

        await using var entities = GetDbContext(true);
        var databaseEntries = await entities.UserInfo
                                          .Where(m => userIds.Contains(m.UserId))
                                          .ToArrayAsync();
        var matchingData = databaseEntries.Join(localData,
                                                db => db.UserId,
                                                mem => (int)mem.InternalUserId,
                                                (db,
                                                 mem) =>
                                                {
                                                    db.CurrentRoles = mem.CurrentRoles;
                                                    db.JoinedDate = mem.JoinedDate;
                                                    return mem;
                                                })
                                          .ToArray();
        if (matchingData.Any())
        {
            await entities.SaveChangesAsync();
            return;
        }

        var missing = localData.Except(matchingData)
                               .ToArray();
        if (missing.Any())
        {
            foreach (var user in missing)
            {
                // Create new user information
                entities.UserInfo.Add(new UserInfo
                {
                    UserId = (int)user.InternalUserId,
                    LastSeen = DateTime.UtcNow,
                    JoinedDate = user.JoinedDate,
                    CurrentRoles = user.CurrentRoles
                });
            }
            await entities.SaveChangesAsync();
        }
    }

    async Task<(InternalUserId UserId, DateTime? LastSeen)[]> IDatabaseAccess.GetLastSeenInfoForUsersAsync(User[] users)
    {
        if (users == null)
            throw new ArgumentNullException(nameof(users));
        if (users.Length == 0)
            throw new ArgumentException("Parameter cannot be empty.", nameof(users));

        var result = new List<(InternalUserId UserId, DateTime? LastSeen)>();
        await using (var entities = GetDbContext())
        {
            foreach (var user in users)
            {
                var ui = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserId == (int)user.InternalUserId);
                result.Add((user.InternalUserId, ui?.LastSeen));
            }
        }

        return result.ToArray();
    }

    async Task IDatabaseAccess.DeleteUserInfoAsync(User user)
    {
        await using var entities = GetDbContext(true);
        var ui = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserId == (int)user.InternalUserId);
        if (ui != null)
        {
            entities.UserInfo.Remove(ui);
            await entities.SaveChangesAsync();
        }
    }

    async Task<bool> IDatabaseAccess.SetBirthdayAsync(User user,
                                                      DateOnly birthday)
    {
        await using var entities = GetDbContext(true);
        var ub = await entities.UserBirthday.SingleOrDefaultAsync(m => m.UserId == (int)user.InternalUserId);
        try
        {
            if (ub != null)
            {
                // Update
                ub.Month = (short)birthday.Month;
                ub.Day = (short)birthday.Day;
            }
            else
            {
                // Insert
                entities.UserBirthday
                        .Add(new UserBirthday
                         {
                            UserId = (int)user.InternalUserId,
                            Month = (short)birthday.Month,
                            Day = (short)birthday.Day
                         });
            }

            await entities.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                             "Failed to set birthday for user {InternalUserId} to {Day}(st/nd/th) {Month}.",
                             user.InternalUserId,
                             birthday.Day,
                             birthday.Month);
            return false;
        }
    }

    async Task<bool> IDatabaseAccess.DeleteUserBirthdayAsync(User user)
    {
        await using var entities = GetDbContext(true);
        var ub = await entities.UserBirthday.SingleOrDefaultAsync(m => m.UserId == (int)user.InternalUserId);
        if (ub == null)
            return false;

        try
        {
            entities.UserBirthday.Remove(ub);
            await entities.SaveChangesAsync();
            return true;
        }
        catch (Exception e)
        {
            _logger.LogError(e, 
                             "Failed to delete birthday for user {InternalUserId}",
                             user.InternalUserId);
            return false;
        }
    }

    async Task<InternalGameId?> IDatabaseAccess.TryGetInternalGameIdAsync(DiscordRoleId primaryGameDiscordRoleId)
    {
        var decDiscordRoleId = (decimal)primaryGameDiscordRoleId;

        await using var context = GetDbContext();
        var game = await context.Game.SingleOrDefaultAsync(m => m.PrimaryGameDiscordRoleId == decDiscordRoleId);

        return game == null
                   ? null
                   : (InternalGameId)game.GameId;
    }

    async Task<InternalGameRoleId?> IDatabaseAccess.TryGetInternalGameRoleIdAsync(DiscordRoleId discordRoleId)
    {
        var decDiscordRoleId = (decimal)discordRoleId;

        await using var context = GetDbContext();
        var gameRole = await context.GameRole.SingleOrDefaultAsync(m => m.DiscordRoleId == decDiscordRoleId);

        return gameRole == null
                   ? null
                   : (InternalGameRoleId)gameRole.GameRoleId;
    }

    async Task<(bool Success, string? Error)> IDatabaseAccess.TryAddGameAsync(InternalUserId userId,
                                                                              DiscordRoleId primaryGameDiscordRoleId)
    {
        var decDiscordRoleId = (decimal)primaryGameDiscordRoleId;

        await using var context = GetDbContext(true);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var matchingGame = await context.Game
                                        .SingleOrDefaultAsync(m => m.PrimaryGameDiscordRoleId == decDiscordRoleId);
        if (matchingGame != null)
            return (false, "A game with the same Discord role Id already exists.");

        context.Game.Add(new Game
        {
            PrimaryGameDiscordRoleId = decDiscordRoleId,
            IncludeInGamesMenu = true,
            IncludeInGuildMembersStatistic = false,
            ModifiedByUserId = (int)userId,
            ModifiedAtTimestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, null);
    }

    async Task<(bool Success, string? Error)> IDatabaseAccess.TryUpdateGameAsync(InternalUserId userId,
                                                                                 InternalGameId internalGameId,
                                                                                 AvailableGame updated)
    {
        var shortGameId = (short)internalGameId;

        await using var context = GetDbContext(true);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var matchingGame = await context.Game.SingleOrDefaultAsync(m => m.GameId == shortGameId);
        if (matchingGame == null)
            return (false, $"Game with the GameId {internalGameId} couldn't be found.");

        matchingGame.IncludeInGuildMembersStatistic = updated.IncludeInGuildMembersStatistic;
        matchingGame.IncludeInGamesMenu = updated.IncludeInGamesMenu;
        matchingGame.GameInterestRoleId = updated.GameInterestRoleId == null
                                              ? null
                                              : (ulong)updated.GameInterestRoleId;
        matchingGame.ModifiedByUserId = (int)userId;
        matchingGame.ModifiedAtTimestamp = DateTime.UtcNow;

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, null);
    }

    async Task<(bool Success, string? Error)> IDatabaseAccess.TryRemoveGameAsync(InternalGameId internalGameId)
    {
        await using var context = GetDbContext(true);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var shortGameId = (short)internalGameId;
        var matchingGame = await context.Game
                                        .Include(m => m.GameRole)
                                        .SingleOrDefaultAsync(m => m.GameId == shortGameId);
        if (matchingGame == null)
            return (false, $"Game with the GameId {shortGameId} couldn't be found.");

        if (matchingGame.GameRole.Any())
        {
            context.GameRole.RemoveRange(matchingGame.GameRole);
            await context.SaveChangesAsync();
        }

        context.Game.Remove(matchingGame);

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, null);
    }

    async Task<(bool Success, string? Error)> IDatabaseAccess.TryAddGameRoleAsync(InternalUserId userId,
                                                                                  InternalGameId internalGameId,
                                                                                  DiscordRoleId discordRoleId)
    {
        await using var context = GetDbContext(true);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var decDiscordRoleId = (decimal)discordRoleId;
        var shortGameId = (short)internalGameId;
        var matchingGameInDb =
            await context.GameRole.SingleOrDefaultAsync(m => m.DiscordRoleId == decDiscordRoleId);
        if (matchingGameInDb != null)
            return (false, matchingGameInDb.GameId == shortGameId
                               ? "The DiscordRoleId is already is use for this game."
                               : "The DiscordRoleId is already in use for another game.");

        var matchingGameRoleName =
            await context.GameRole.AnyAsync(m => m.GameId == shortGameId && m.DiscordRoleId == decDiscordRoleId);
        if (matchingGameRoleName)
            return (false, "A role with the same name is already assigned to the game.");

        context.GameRole.Add(new GameRole
        {
            GameId = shortGameId,
            DiscordRoleId = decDiscordRoleId,
            ModifiedByUserId = (int)userId,
            ModifiedAtTimestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, null);
    }

    async Task<(bool Success, string? Error)> IDatabaseAccess.TryRemoveGameRoleAsync(InternalGameRoleId gameRoleId)
    {
        await using var context = GetDbContext(true);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var gameRole = await context.GameRole.SingleOrDefaultAsync(m => m.GameRoleId == (short)gameRoleId);
        if (gameRole == null)
            return (false, "Couldn't find game role by Id.");

        context.GameRole.Remove(gameRole);

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, null);
    }

    async Task<InternalUserId[]> IDatabaseAccess.GetUsersWithBirthdayAsync(short month,
                                                                           short day)
    {
        await using var context = GetDbContext(true);
        return (await context.UserBirthday
                             .Where(m => m.Month == month
                                      && m.Day == day)
                             .Select(m => m.UserId)
                             .ToArrayAsync())
              .Select(m => (InternalUserId)m)
              .ToArray();
    }
}