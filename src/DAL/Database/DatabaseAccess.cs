namespace HoU.GuildBot.DAL.Database
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Model;
    using Shared.DAL;
    using Shared.Objects;
    using Shared.StrongTypes;
    using User = Shared.Objects.User;

    [UsedImplicitly]
    public class DatabaseAccess : IDatabaseAccess
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<DatabaseAccess> _logger;
        private readonly AppSettings _appSettings;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public DatabaseAccess(ILogger<DatabaseAccess> logger,
                              AppSettings appSettings)
        {
            _logger = logger;
            _appSettings = appSettings;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private HoUEntities GetEntities() => new HoUEntities(_appSettings.ConnectionString);

        private static User ToPoco(Model.User user)
        {
            return new User((InternalUserID) user.UserID, (DiscordUserID) user.DiscordUserID);
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IDatabaseAccess Members

        async Task<User[]> IDatabaseAccess.GetAllUsers()
        {
            Model.User[] dbObjects;
            using (var entities = GetEntities())
            {
                dbObjects = await entities.User.ToArrayAsync().ConfigureAwait(false);
            }

            return dbObjects.Select(ToPoco).ToArray();
        }

        async Task IDatabaseAccess.AddUsers(IEnumerable<DiscordUserID> userIDs)
        {
            using (var entities = GetEntities())
            {
                var existingUserIDs = await entities.User.Select(m => m.DiscordUserID).ToArrayAsync().ConfigureAwait(false);
                var missingUserIDs = userIDs.Except(existingUserIDs.Select(m => (DiscordUserID)(ulong)m)).ToArray();

                if (!missingUserIDs.Any())
                    return;

                _logger.LogInformation($"Adding {missingUserIDs.Length} missing users to the database...");
                var added = 0;
                foreach (var missingUserID in missingUserIDs)
                {
                    entities.User.Add(new Model.User
                    {
                        DiscordUserID = (decimal)missingUserID
                    });
                    added++;
                }

                await entities.SaveChangesAsync().ConfigureAwait(false);
                _logger.LogInformation($"Added {added} missing users to the database.");
            }
        }

        async Task<(User User, bool IsNew)> IDatabaseAccess.GetOrAddUser(DiscordUserID userID)
        {
            using (var entities = GetEntities())
            {
                var decUserID = (decimal)userID;
                var dbObject = await entities.User.SingleOrDefaultAsync(m => m.DiscordUserID == decUserID).ConfigureAwait(false);
                if (dbObject != null)
                    return (ToPoco(dbObject), false);

                // Add missing user
                var newUserObject = new Model.User
                {
                    DiscordUserID = decUserID
                };
                entities.User.Add(newUserObject);

                await entities.SaveChangesAsync().ConfigureAwait(false);
                return (ToPoco(newUserObject), true);
            }
        }

        async Task<(string Name, string Description, string Content)[]> IDatabaseAccess.GetAllMessages()
        {
            using (var entities = GetEntities())
            {
                var local = await entities.Message.ToArrayAsync().ConfigureAwait(false);
                return local.Select(m => (m.Name, m.Description, m.Content)).ToArray();
            }
        }

        async Task<string> IDatabaseAccess.GetMessageContent(string name)
        {
            using (var entities = GetEntities())
            {
                var match = await entities.Message.SingleOrDefaultAsync(m => m.Name == name).ConfigureAwait(false);
                return match?.Content;
            }
        }

        async Task<bool> IDatabaseAccess.SetMessageContent(string name, string content)
        {
            using (var entities = GetEntities())
            {
                var match = await entities.Message.SingleOrDefaultAsync(m => m.Name == name).ConfigureAwait(false);
                if (match == null)
                    return false;

                match.Content = content;
                await entities.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
        }

        async Task<bool> IDatabaseAccess.AddVacation(User user, DateTime start, DateTime end, string note)
        {
            var internalUserId = (int) user.InternalUserID;
            using (var entities = GetEntities())
            {
                // Check if colliding entry exists
                var collisions = await entities.Vacation
                                               .AnyAsync(m => m.UserID == internalUserId
                                                           && end >= m.Start
                                                           && start <= m.End)
                                               .ConfigureAwait(false);
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
                await entities.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
        }

        async Task<bool> IDatabaseAccess.DeleteVacation(User user, DateTime start, DateTime end)
        {
            using (var entities = GetEntities())
            {
                // Find the maching vacation
                var match = await entities.Vacation
                                          .SingleOrDefaultAsync(m => m.UserID == (int)user.InternalUserID
                                                                  && m.Start == start
                                                                  && m.End == end)
                                          .ConfigureAwait(false);
                if (match == null)
                    return false;

                entities.Vacation.Remove(match);
                await entities.SaveChangesAsync().ConfigureAwait(false);
                return true;
            }
        }

        async Task IDatabaseAccess.DeletePastVacations()
        {
            using (var entities = GetEntities())
            {
                // Get past vacations - we don't need to keep those in the database
                var pastVacations = await entities.Vacation.Where(m => m.End < DateTime.Today).ToArrayAsync().ConfigureAwait(false);
                if (!pastVacations.Any())
                    return;
                // If any vacations in the past are still in the database, delete them
                entities.Vacation.RemoveRange(pastVacations);
                await entities.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        async Task IDatabaseAccess.DeleteVacations(User user)
        {
            using (var entities = GetEntities())
            {
                var vacations = await entities.Vacation.Where(m => m.UserID == (int)user.InternalUserID).ToArrayAsync().ConfigureAwait(false);
                if (!vacations.Any())
                    return;
                // If the user has any vacations in the database, delete them
                entities.Vacation.RemoveRange(vacations);
                await entities.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        async Task<(DiscordUserID UserID, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacations()
        {
            using (var entities = GetEntities())
            {
                var localItems = await entities.Vacation
                                               .Where(m => m.End >= DateTime.Today)
                                               .Join(entities.User, vacation => vacation.UserID, u => u.UserID, (vacation, u) => new {u.DiscordUserID, vacation.Start, vacation.End, vacation.Note})
                                               .ToArrayAsync()
                                               .ConfigureAwait(false);
                return localItems.Select(m => ((DiscordUserID)m.DiscordUserID, m.Start, m.End, m.Note)).ToArray();
            }
        }

        async Task<(DiscordUserID UserID, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacations(User user)
        {
            using (var entities = GetEntities())
            {
                var localItems = await entities.Vacation
                                               .Where(m => m.End >= DateTime.Today)
                                               .Join(entities.User, vacation => vacation.UserID, u => u.UserID, (vacation, u) => new { u.UserID, u.DiscordUserID, vacation.Start, vacation.End, vacation.Note })
                                               .Where(m => m.UserID == (int)user.InternalUserID)
                                               .ToArrayAsync()
                                               .ConfigureAwait(false);
                return localItems.Select(m => ((DiscordUserID)m.DiscordUserID, m.Start, m.End, m.Note)).ToArray();
            }
        }

        async Task<(DiscordUserID UserID, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacations(DateTime date)
        {
            using (var entities = GetEntities())
            {
                var localItems = await entities.Vacation
                                               .Where(m => date >= m.Start
                                                        && date <= m.End)
                                               .Join(entities.User, vacation => vacation.UserID, user => user.UserID, (vacation, user) => new { user.DiscordUserID, vacation.Start, vacation.End, vacation.Note })
                                               .ToArrayAsync()
                                               .ConfigureAwait(false);
                return localItems.Select(m => ((DiscordUserID)m.DiscordUserID, m.Start, m.End, m.Note)).ToArray();
            }
        }

        async Task<AvailableGame[]> IDatabaseAccess.GetAvailableGames()
        {
            using (var entities = GetEntities())
            {
                var localItems = await entities.Game.ToArrayAsync().ConfigureAwait(false);
                return localItems.Select(m => new AvailableGame
                {
                    LongName = m.LongName,
                    ShortName = m.ShortName
                }).ToArray();
            }
        }

        async Task IDatabaseAccess.UpdateUserInfoLastSeen(User user, DateTime lastSeen)
        {
            using (var entities = GetEntities())
            {
                var existingInfo = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserID == (decimal)user.InternalUserID).ConfigureAwait(false);
                if (existingInfo != null)
                {
                    existingInfo.LastSeen = lastSeen;
                    await entities.SaveChangesAsync().ConfigureAwait(false);
                    return;
                }

                // Create new user info
                entities.UserInfo.Add(new UserInfo {UserID = (int)user.InternalUserID, LastSeen = lastSeen});
                await entities.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        async Task<(InternalUserID UserID, DateTime? LastSeen)[]> IDatabaseAccess.GetLastSeenInfoForUsers(User[] users)
        {
            if (users == null)
                throw new ArgumentNullException(nameof(users));
            if (users.Length == 0)
                throw new ArgumentException("Parameter cannot be empty.", nameof(users));

            var result = new List<(InternalUserID UserID, DateTime? LastSeen)>();
            using (var entities = GetEntities())
            {
                foreach (var user in users)
                {
                    var ui = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserID == (int)user.InternalUserID).ConfigureAwait(false);
                    result.Add((user.InternalUserID, ui?.LastSeen));
                }
            }

            return result.ToArray();
        }

        async Task IDatabaseAccess.DeleteUserInfo(User user)
        {
            using (var entities = GetEntities())
            {
                var ui = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserID == (int)user.InternalUserID).ConfigureAwait(false);
                if (ui != null)
                {
                    entities.UserInfo.Remove(ui);
                    await entities.SaveChangesAsync().ConfigureAwait(false);
                }
            }
        }

        #endregion
    }
}