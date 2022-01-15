using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using HoU.GuildBot.DAL.Database.Model;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using HoU.GuildBot.Shared.StrongTypes;
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
        builder.UseSqlServer(rootSettings.ConnectionStringForOwnDatabase);
        _trackingContextOptions = builder.Options;
        builder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        _noTrackingContextOptions = builder.Options;
    }

    private HandOfUnityContext GetDbContext(bool withChangeTracking = false) =>
        new(withChangeTracking ? _trackingContextOptions : _noTrackingContextOptions);

    private static User Map(Model.User user) =>
        new((InternalUserId)user.UserID,
            (DiscordUserId)user.DiscordUserId);

    async Task<User[]> IDatabaseAccess.GetAllUsers()
    {
        Model.User[] dbObjects;
        await using (var entities = GetDbContext())
        {
            dbObjects = await entities.User.ToArrayAsync();
        }

        return dbObjects.Select(Map).ToArray();
    }

    async Task IDatabaseAccess.EnsureUsersExistAsync(IEnumerable<DiscordUserId> userIDs)
    {
        await using var entities = GetDbContext(true);
        var existingUserIDs = await entities.User.Select(m => m.DiscordUserId).ToArrayAsync();
        var missingUserIDs = userIDs.Except(existingUserIDs.Select(m => (DiscordUserId)(ulong)m)).ToArray();

        if (!missingUserIDs.Any())
            return;

        _logger.LogInformation("Adding {AmountOfMissingUsers} missing users to the database...", missingUserIDs.Length);
        var added = 0;
        foreach (var missingUserID in missingUserIDs)
        {
            entities.User.Add(new Model.User
            {
                DiscordUserId = (decimal)missingUserID
            });
            added++;
        }

        await entities.SaveChangesAsync();
        _logger.LogInformation("Added {UsersAdded} missing users to the database.", added);
    }

    async Task<(User User, bool IsNew)> IDatabaseAccess.GetOrAddUserAsync(DiscordUserId userID)
    {
        await using var entities = GetDbContext(true);
        var decUserID = (decimal)userID;
        var dbObject = await entities.User.SingleOrDefaultAsync(m => m.DiscordUserId == decUserID);
        if (dbObject != null)
            return (Map(dbObject), false);

        // Add missing user
        var newUserObject = new Model.User
        {
            DiscordUserId = decUserID
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
                                       .AnyAsync(m => m.UserID == internalUserId
                                                   && end >= m.Start
                                                   && start <= m.End)
            ;
        if (collisions)
            return false;

        // If no colliding entry exists, we can add the entry
        entities.Vacation.Add(new Vacation
        {
            UserID = internalUserId,
            Start = start,
            End = end,
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
                                  .SingleOrDefaultAsync(m => m.UserID == (int)user.InternalUserId
                                                          && m.Start == start
                                                          && m.End == end)
            ;
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
        var pastVacations = await entities.Vacation.Where(m => m.End < DateTime.Today).ToArrayAsync();
        if (!pastVacations.Any())
            return;
        // If any vacations in the past are still in the database, delete them
        entities.Vacation.RemoveRange(pastVacations);
        await entities.SaveChangesAsync();
    }

    async Task IDatabaseAccess.DeleteVacationsAsync(User user)
    {
        await using var entities = GetDbContext(true);
        var vacations = await entities.Vacation.Where(m => m.UserID == (int)user.InternalUserId).ToArrayAsync();
        if (!vacations.Any())
            return;
        // If the user has any vacations in the database, delete them
        entities.Vacation.RemoveRange(vacations);
        await entities.SaveChangesAsync();
    }

    async Task<(DiscordUserId UserId, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacationsAsync()
    {
        await using var entities = GetDbContext();
        var localItems = await entities.Vacation
                                       .Where(m => m.End >= DateTime.Today)
                                       .Join(entities.User,
                                             vacation => vacation.UserID,
                                             u => u.UserID,
                                             (vacation,
                                              u) => new { u.DiscordUserId, vacation.Start, vacation.End, vacation.Note })
                                       .ToArrayAsync()
            ;
        return localItems.Select(m => ((DiscordUserId)m.DiscordUserId, m.Start, m.End, m.Note)).ToArray();
    }

    async Task<(DiscordUserId UserId, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacationsAsync(User user)
    {
        await using var entities = GetDbContext();
        var localItems = await entities.Vacation
                                       .Where(m => m.End >= DateTime.Today)
                                       .Join(entities.User,
                                             vacation => vacation.UserID,
                                             u => u.UserID,
                                             (vacation,
                                              u) => new { u.UserID, u.DiscordUserId, vacation.Start, vacation.End, vacation.Note })
                                       .Where(m => m.UserID == (int)user.InternalUserId)
                                       .ToArrayAsync()
            ;
        return localItems.Select(m => ((DiscordUserId)m.DiscordUserId, m.Start, m.End, m.Note)).ToArray();
    }

    async Task<(DiscordUserId UserId, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacationsAsync(DateTime date)
    {
        await using var entities = GetDbContext();
        var localItems = await entities.Vacation
                                       .Where(m => date >= m.Start
                                                && date <= m.End)
                                       .Join(entities.User,
                                             vacation => vacation.UserID,
                                             user => user.UserID,
                                             (vacation,
                                              user) => new { user.DiscordUserId, vacation.Start, vacation.End, vacation.Note })
                                       .ToArrayAsync()
            ;
        return localItems.Select(m => ((DiscordUserId)m.DiscordUserId, m.Start, m.End, m.Note)).ToArray();
    }

    async Task<AvailableGame[]> IDatabaseAccess.GetAvailableGamesAsync()
    {
        await using var entities = GetDbContext();
        var localItems = await entities.Game.Include(m => m.GameRole).ToArrayAsync();
        return localItems.Select(m =>
        {
            var g = new AvailableGame
            {
                PrimaryGameDiscordRoleId = (DiscordRoleId)(ulong)m.PrimaryGameDiscordRoleID,
                IncludeInGuildMembersStatistic = m.IncludeInGuildMembersStatistic,
                IncludeInGamesMenu = m.IncludeInGamesMenu,
                GameInterestRoleId = m.GameInterestRoleId == null
                                         ? null
                                         : (DiscordRoleId)(ulong)m.GameInterestRoleId
            };
            g.AvailableRoles.AddRange(m.GameRole.Select(n => new AvailableGameRole
                                                            { DiscordRoleId = (DiscordRoleId)Convert.ToUInt64(n.DiscordRoleID) }));
            return g;
        }).ToArray();
    }

    async Task IDatabaseAccess.UpdateUserInfoLastSeenAsync(User user,
                                                           DateTime lastSeen)
    {
        await using var entities = GetDbContext(true);
        var existingInfo = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserID == (decimal)user.InternalUserId);
        if (existingInfo != null)
        {
            existingInfo.LastSeen = lastSeen;
            await entities.SaveChangesAsync();
            return;
        }

        // Create new user info
        entities.UserInfo.Add(new UserInfo { UserID = (int)user.InternalUserId, LastSeen = lastSeen });
        await entities.SaveChangesAsync();
    }

    async Task<(InternalUserId UserId, DateTime? LastSeen)[]> IDatabaseAccess.GetLastSeenInfoForUsersAsync(User[] users)
    {
        if (users == null)
            throw new ArgumentNullException(nameof(users));
        if (users.Length == 0)
            throw new ArgumentException("Parameter cannot be empty.", nameof(users));

        var result = new List<(InternalUserId UserID, DateTime? LastSeen)>();
        await using (var entities = GetDbContext())
        {
            foreach (var user in users)
            {
                var ui = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserID == (int)user.InternalUserId);
                result.Add((user.InternalUserId, ui?.LastSeen));
            }
        }

        return result.ToArray();
    }

    async Task IDatabaseAccess.DeleteUserInfoAsync(User user)
    {
        await using var entities = GetDbContext(true);
        var ui = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserID == (int)user.InternalUserId);
        if (ui != null)
        {
            entities.UserInfo.Remove(ui);
            await entities.SaveChangesAsync();
        }
    }

    async Task<InternalGameId?> IDatabaseAccess.TryGetInternalGameIdAsync(DiscordRoleId primaryGameDiscordRoleID)
    {
        var decDiscordRoleID = (decimal)primaryGameDiscordRoleID;

        await using var context = GetDbContext();
        var game = await context.Game.SingleOrDefaultAsync(m => m.PrimaryGameDiscordRoleID == decDiscordRoleID);

        return game == null
                   ? null
                   : (InternalGameId)game.GameID;
    }

    async Task<InternalGameRoleId?> IDatabaseAccess.TryGetInternalGameRoleIdAsync(DiscordRoleId discordRoleID)
    {
        var decDiscordRoleID = (decimal)discordRoleID;

        await using var context = GetDbContext();
        var gameRole = await context.GameRole.SingleOrDefaultAsync(m => m.DiscordRoleID == decDiscordRoleID);

        return gameRole == null
                   ? null
                   : (InternalGameRoleId)gameRole.GameRoleID;
    }

    async Task<(bool Success, string? Error)> IDatabaseAccess.TryAddGameAsync(InternalUserId userID,
                                                                              DiscordRoleId primaryGameDiscordRoleID)
    {
        var decDiscordRoleID = (decimal)primaryGameDiscordRoleID;

        await using var context = GetDbContext(true);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var matchingGame = await context.Game
                                        .SingleOrDefaultAsync(m => m.PrimaryGameDiscordRoleID == decDiscordRoleID);
        if (matchingGame != null)
            return (false, "A game with the same Discord role Id already exists.");

        context.Game.Add(new Game
        {
            PrimaryGameDiscordRoleID = decDiscordRoleID,
            IncludeInGamesMenu = true,
            IncludeInGuildMembersStatistic = false,
            ModifiedByUserID = (int)userID,
            ModifiedAtTimestamp = DateTime.UtcNow
        });

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, null);
    }

    async Task<(bool Success, string? Error)> IDatabaseAccess.TryUpdateGameAsync(InternalUserId userID,
                                                                                 InternalGameId internalGameId,
                                                                                 AvailableGame updated)
    {
        var shortGameId = (short)internalGameId;

        await using var context = GetDbContext(true);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var matchingGame = await context.Game.SingleOrDefaultAsync(m => m.GameID == shortGameId);
        if (matchingGame == null)
            return (false, $"Game with the GameID {internalGameId} couldn't be found.");

        matchingGame.IncludeInGuildMembersStatistic = updated.IncludeInGuildMembersStatistic;
        matchingGame.IncludeInGamesMenu = updated.IncludeInGamesMenu;
        matchingGame.GameInterestRoleId = updated.GameInterestRoleId == null
                                              ? null
                                              : (ulong)updated.GameInterestRoleId;
        matchingGame.ModifiedByUserID = (int)userID;
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
                                        .SingleOrDefaultAsync(m => m.GameID == shortGameId);
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

    async Task<(bool Success, string? Error)> IDatabaseAccess.TryAddGameRoleAsync(InternalUserId userID,
                                                                                  InternalGameId internalGameId,
                                                                                  DiscordRoleId discordRoleID)
    {
        await using var context = GetDbContext(true);
        await using var transaction = await context.Database.BeginTransactionAsync();

        var decDiscordRoleID = (decimal)discordRoleID;
        var shortGameId = (short)internalGameId;
        var matchingGameInDb =
            await context.GameRole.SingleOrDefaultAsync(m => m.DiscordRoleID == decDiscordRoleID);
        if (matchingGameInDb != null)
            return (false, matchingGameInDb.GameID == shortGameId
                               ? "The DiscordRoleID is already is use for this game."
                               : "The DiscordRoleID is already in use for another game.");

        var matchingGameRoleName =
            await context.GameRole.AnyAsync(m => m.GameID == shortGameId && m.DiscordRoleID == decDiscordRoleID);
        if (matchingGameRoleName)
            return (false, "A role with the same name is already assigned to the game.");

        context.GameRole.Add(new GameRole
        {
            GameID = shortGameId,
            DiscordRoleID = decDiscordRoleID,
            ModifiedByUserID = (int)userID,
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

        var gameRole = await context.GameRole.SingleOrDefaultAsync(m => m.GameRoleID == (short)gameRoleId);
        if (gameRole == null)
            return (false, "Couldn't find game role by ID.");

        context.GameRole.Remove(gameRole);

        await context.SaveChangesAsync();
        await transaction.CommitAsync();

        return (true, null);
    }
}