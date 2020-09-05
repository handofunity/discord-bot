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

namespace HoU.GuildBot.DAL.Database
{
    [UsedImplicitly]
    public class DatabaseAccess : IDatabaseAccess
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Fields

        private readonly ILogger<DatabaseAccess> _logger;
        private readonly DbContextOptions<HandOfUnityContext> _handOfUnityContextOptions;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Constructors

        public DatabaseAccess(ILogger<DatabaseAccess> logger,
                              AppSettings appSettings)
        {
            _logger = logger;
            var builder = new DbContextOptionsBuilder<HandOfUnityContext>();
            builder.UseSqlServer(appSettings.HandOfUnityConnectionString);
            _handOfUnityContextOptions = builder.Options;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region Private Methods

        private HandOfUnityContext GetDbContext() => new HandOfUnityContext(_handOfUnityContextOptions);

        private static User ToPoCo(Model.User user)
        {
            return new User((InternalUserID) user.UserID, (DiscordUserID) user.DiscordUserID);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////

        #region IDatabaseAccess Members

        async Task<User[]> IDatabaseAccess.GetAllUsers()
        {
            Model.User[] dbObjects;
            await using (var entities = GetDbContext())
            {
                dbObjects = await entities.User.AsQueryable().ToArrayAsync();
            }

            return dbObjects.Select(ToPoCo).ToArray();
        }

        async Task IDatabaseAccess.EnsureUsersExist(IEnumerable<DiscordUserID> userIDs)
        {
            await using var entities = GetDbContext();
            var existingUserIDs = await entities.User.AsQueryable().Select(m => m.DiscordUserID).ToArrayAsync();
            var missingUserIDs = userIDs.Except(existingUserIDs.Select(m => (DiscordUserID) (ulong) m)).ToArray();

            if (!missingUserIDs.Any())
                return;

            _logger.LogInformation($"Adding {missingUserIDs.Length} missing users to the database...");
            var added = 0;
            foreach (var missingUserID in missingUserIDs)
            {
                entities.User.Add(new Model.User
                {
                    DiscordUserID = (decimal) missingUserID
                });
                added++;
            }

            await entities.SaveChangesAsync();
            _logger.LogInformation($"Added {added} missing users to the database.");
        }

        async Task<(User User, bool IsNew)> IDatabaseAccess.GetOrAddUser(DiscordUserID userID)
        {
            await using var entities = GetDbContext();
            var decUserID = (decimal) userID;
            var dbObject = await entities.User.AsQueryable().SingleOrDefaultAsync(m => m.DiscordUserID == decUserID);
            if (dbObject != null)
                return (ToPoCo(dbObject), false);

            // Add missing user
            var newUserObject = new Model.User
            {
                DiscordUserID = decUserID
            };
            entities.User.Add(newUserObject);

            await entities.SaveChangesAsync();
            return (ToPoCo(newUserObject), true);
        }

        async Task<(string Name, string Description, string Content)[]> IDatabaseAccess.GetAllMessages()
        {
            await using var entities = GetDbContext();
            var local = await entities.Message.AsQueryable().ToArrayAsync();
            return local.Select(m => (m.Name, m.Description, m.Content)).ToArray();
        }

        async Task<string> IDatabaseAccess.GetMessageContent(string name)
        {
            await using var entities = GetDbContext();
            var match = await entities.Message.AsQueryable().SingleOrDefaultAsync(m => m.Name == name);
            return match?.Content;
        }

        async Task<bool> IDatabaseAccess.SetMessageContent(string name,
                                                           string content)
        {
            await using var entities = GetDbContext();
            var match = await entities.Message.AsQueryable().SingleOrDefaultAsync(m => m.Name == name);
            if (match == null)
                return false;

            match.Content = content;
            await entities.SaveChangesAsync();
            return true;
        }

        async Task<bool> IDatabaseAccess.AddVacation(User user,
                                                     DateTime start,
                                                     DateTime end,
                                                     string note)
        {
            var internalUserId = (int) user.InternalUserID;
            await using var entities = GetDbContext();
            // Check if colliding entry exists
            var collisions = await entities.Vacation
                                           .AsQueryable()
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
                Note = string.IsNullOrWhiteSpace(note) ? null : note.Substring(0, Math.Min(note.Length, 1024))
            });
            await entities.SaveChangesAsync();
            return true;
        }

        async Task<bool> IDatabaseAccess.DeleteVacation(User user,
                                                        DateTime start,
                                                        DateTime end)
        {
            await using var entities = GetDbContext();
            // Find the matching vacation
            var match = await entities.Vacation
                                      .AsQueryable()
                                      .SingleOrDefaultAsync(m => m.UserID == (int) user.InternalUserID
                                                                 && m.Start == start
                                                                 && m.End == end)
                ;
            if (match == null)
                return false;

            entities.Vacation.Remove(match);
            await entities.SaveChangesAsync();
            return true;
        }

        async Task IDatabaseAccess.DeletePastVacations()
        {
            await using var entities = GetDbContext();
            // Get past vacations - we don't need to keep those in the database
            var pastVacations = await entities.Vacation.AsQueryable().Where(m => m.End < DateTime.Today).ToArrayAsync();
            if (!pastVacations.Any())
                return;
            // If any vacations in the past are still in the database, delete them
            entities.Vacation.RemoveRange(pastVacations);
            await entities.SaveChangesAsync();
        }

        async Task IDatabaseAccess.DeleteVacations(User user)
        {
            await using var entities = GetDbContext();
            var vacations = await entities.Vacation.AsQueryable().Where(m => m.UserID == (int) user.InternalUserID).ToArrayAsync();
            if (!vacations.Any())
                return;
            // If the user has any vacations in the database, delete them
            entities.Vacation.RemoveRange(vacations);
            await entities.SaveChangesAsync();
        }

        async Task<(DiscordUserID UserID, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacations()
        {
            await using var entities = GetDbContext();
            var localItems = await entities.Vacation
                                           .AsQueryable()
                                           .Where(m => m.End >= DateTime.Today)
                                           .Join(entities.User,
                                                 vacation => vacation.UserID,
                                                 u => u.UserID,
                                                 (vacation,
                                                  u) => new {u.DiscordUserID, vacation.Start, vacation.End, vacation.Note})
                                           .ToArrayAsync()
                ;
            return localItems.Select(m => ((DiscordUserID) m.DiscordUserID, m.Start, m.End, m.Note)).ToArray();
        }

        async Task<(DiscordUserID UserID, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacations(User user)
        {
            await using var entities = GetDbContext();
            var localItems = await entities.Vacation
                                           .AsQueryable()
                                           .Where(m => m.End >= DateTime.Today)
                                           .Join(entities.User,
                                                 vacation => vacation.UserID,
                                                 u => u.UserID,
                                                 (vacation,
                                                  u) => new {u.UserID, u.DiscordUserID, vacation.Start, vacation.End, vacation.Note})
                                           .Where(m => m.UserID == (int) user.InternalUserID)
                                           .ToArrayAsync()
                ;
            return localItems.Select(m => ((DiscordUserID) m.DiscordUserID, m.Start, m.End, m.Note)).ToArray();
        }

        async Task<(DiscordUserID UserID, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacations(DateTime date)
        {
            await using var entities = GetDbContext();
            var localItems = await entities.Vacation
                                           .AsQueryable()
                                           .Where(m => date >= m.Start
                                                       && date <= m.End)
                                           .Join(entities.User,
                                                 vacation => vacation.UserID,
                                                 user => user.UserID,
                                                 (vacation,
                                                  user) => new {user.DiscordUserID, vacation.Start, vacation.End, vacation.Note})
                                           .ToArrayAsync()
                ;
            return localItems.Select(m => ((DiscordUserID) m.DiscordUserID, m.Start, m.End, m.Note)).ToArray();
        }

        async Task<AvailableGame[]> IDatabaseAccess.GetAvailableGames()
        {
            await using var entities = GetDbContext();
            var localItems = await entities.Game.Include(m => m.GameRole).ToArrayAsync();
            return localItems.Select(m =>
            {
                var g = new AvailableGame
                {
                    LongName = m.LongName,
                    ShortName = m.ShortName,
                    PrimaryGameDiscordRoleID = m.PrimaryGameDiscordRoleID == null ? null : (ulong?) m.PrimaryGameDiscordRoleID,
                    IncludeInGuildMembersStatistic = m.IncludeInGuildMembersStatistic,
                    IncludeInGamesMenu = m.IncludeInGamesMenu
                };
                g.AvailableRoles.AddRange(m.GameRole.Select(n => new AvailableGameRole
                {
                    DiscordRoleID = Convert.ToUInt64(n.DiscordRoleID),
                    RoleName = n.RoleName
                }));
                return g;
            }).ToArray();
        }

        async Task IDatabaseAccess.UpdateUserInfoLastSeen(User user,
                                                          DateTime lastSeen)
        {
            await using var entities = GetDbContext();
            var existingInfo = await entities.UserInfo.AsQueryable()
                                             .SingleOrDefaultAsync(m => m.UserID == (decimal) user.InternalUserID);
            if (existingInfo != null)
            {
                existingInfo.LastSeen = lastSeen;
                await entities.SaveChangesAsync();
                return;
            }

            // Create new user info
            entities.UserInfo.Add(new UserInfo {UserID = (int) user.InternalUserID, LastSeen = lastSeen});
            await entities.SaveChangesAsync();
        }

        async Task<(InternalUserID UserID, DateTime? LastSeen)[]> IDatabaseAccess.GetLastSeenInfoForUsers(User[] users)
        {
            if (users == null)
                throw new ArgumentNullException(nameof(users));
            if (users.Length == 0)
                throw new ArgumentException("Parameter cannot be empty.", nameof(users));

            var result = new List<(InternalUserID UserID, DateTime? LastSeen)>();
            await using (var entities = GetDbContext())
            {
                foreach (var user in users)
                {
                    var ui = await entities.UserInfo.AsQueryable().SingleOrDefaultAsync(m => m.UserID == (int) user.InternalUserID);
                    result.Add((user.InternalUserID, ui?.LastSeen));
                }
            }

            return result.ToArray();
        }

        async Task IDatabaseAccess.DeleteUserInfo(User user)
        {
            await using var entities = GetDbContext();
            var ui = await entities.UserInfo.AsQueryable().SingleOrDefaultAsync(m => m.UserID == (int) user.InternalUserID);
            if (ui != null)
            {
                entities.UserInfo.Remove(ui);
                await entities.SaveChangesAsync();
            }
        }

        async Task<short?> IDatabaseAccess.TryGetGameID(string shortName)
        {
            await using var context = GetDbContext();
            var game = await context.Game.AsQueryable().SingleOrDefaultAsync(m => m.ShortName == shortName);
            return game?.GameID;
        }

        async Task<(short ID, string CurrentName)?> IDatabaseAccess.TryGetGameRole(ulong discordRoleID)
        {
            await using var context = GetDbContext();
            var decDiscordRoleID = (decimal) discordRoleID;
            var gameRole = await context.GameRole.AsQueryable().SingleOrDefaultAsync(m => m.DiscordRoleID == decDiscordRoleID);
            if (gameRole == null)
                return null;
            return (gameRole.GameRoleID, gameRole.RoleName);
        }

        async Task<(bool Success, string Error)> IDatabaseAccess.TryAddGame(InternalUserID userID,
                                                                            string gameLongName,
                                                                            string gameShortName,
                                                                            ulong? primaryGameDiscordRoleID)
        {
            await using var context = GetDbContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var matchingGame = await context.Game.AsQueryable()
                                            .SingleOrDefaultAsync(m => m.LongName == gameLongName || m.ShortName == gameShortName);
            if (matchingGame != null)
                return (false, "A game with the same long or short name already exists.");

            context.Game.Add(new Game
            {
                LongName = gameLongName,
                ShortName = gameShortName,
                ModifiedByUserID = (int) userID,
                ModifiedAtTimestamp = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, null);
        }

        async Task<(bool Success, string Error)> IDatabaseAccess.TryEditGame(InternalUserID userID,
                                                                             short gameID,
                                                                             AvailableGame updated)
        {
            await using var context = GetDbContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var matchingGame = await context.Game.AsQueryable().SingleOrDefaultAsync(m => m.GameID == gameID);
            if (matchingGame == null)
                return (false, $"Game with the GameID {gameID} couldn't be found.");

            matchingGame.LongName = updated.LongName;
            matchingGame.ShortName = updated.ShortName;
            matchingGame.PrimaryGameDiscordRoleID = updated.PrimaryGameDiscordRoleID;
            matchingGame.IncludeInGuildMembersStatistic = updated.IncludeInGuildMembersStatistic;
            matchingGame.IncludeInGamesMenu = updated.IncludeInGamesMenu;
            matchingGame.ModifiedByUserID = (int) userID;
            matchingGame.ModifiedAtTimestamp = DateTime.UtcNow;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, null);
        }

        async Task<(bool Success, string Error)> IDatabaseAccess.TryRemoveGame(short gameID)
        {
            await using var context = GetDbContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var matchingGame = await context.Game
                                            .Include(m => m.GameRole)
                                            .SingleOrDefaultAsync(m => m.GameID == gameID)
                ;
            if (matchingGame == null)
                return (false, $"Game with the GameID {gameID} couldn't be found.");

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

        async Task<(bool Success, string Error)> IDatabaseAccess.TryAddGameRole(InternalUserID userID,
                                                                                short gameID,
                                                                                string roleName,
                                                                                ulong discordRoleID)
        {
            await using var context = GetDbContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var decDiscordRoleID = (decimal) discordRoleID;
            var matchingDiscordRoleID =
                await context.GameRole.AsQueryable().SingleOrDefaultAsync(m => m.DiscordRoleID == decDiscordRoleID);
            if (matchingDiscordRoleID != null)
                return (false, matchingDiscordRoleID.GameID == gameID
                                   ? "The DiscordRoleID is already is use for this game."
                                   : "The DiscordRoleID is already in use for another game.");

            var matchingGameRoleName =
                await context.GameRole.AsQueryable().AnyAsync(m => m.GameID == gameID && m.RoleName == roleName);
            if (matchingGameRoleName)
                return (false, "A role with the same name is already assigned to the game.");

            context.GameRole.Add(new GameRole
            {
                GameID = gameID,
                RoleName = roleName,
                DiscordRoleID = decDiscordRoleID,
                ModifiedByUserID = (int) userID,
                ModifiedAtTimestamp = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, null);
        }

        async Task<(bool Success, string Error)> IDatabaseAccess.TryEditGameRole(InternalUserID userID,
                                                                                 short gameRoleID,
                                                                                 string newRoleName)
        {
            await using var context = GetDbContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var gameRole = await context.GameRole.AsQueryable().SingleOrDefaultAsync(m => m.GameRoleID == gameRoleID);
            if (gameRole == null)
                return (false, "Couldn't find game role by ID.");

            gameRole.RoleName = newRoleName;
            gameRole.ModifiedByUserID = (int) userID;
            gameRole.ModifiedAtTimestamp = DateTime.UtcNow;

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, null);
        }

        async Task<(bool Success, string Error)> IDatabaseAccess.TryRemoveGameRole(short gameRoleID)
        {
            await using var context = GetDbContext();
            await using var transaction = await context.Database.BeginTransactionAsync();

            var gameRole = await context.GameRole.AsQueryable().SingleOrDefaultAsync(m => m.GameRoleID == gameRoleID);
            if (gameRole == null)
                return (false, "Couldn't find game role by ID.");

            context.GameRole.Remove(gameRole);

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            return (true, null);
        }

        #endregion
    }
}