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

        private static async Task<int> GetInternalUserId(HoUEntities entities, ulong discordUserId)
        {
            var user = await entities.User.SingleOrDefaultAsync(m => m.DiscordUserID == discordUserId).ConfigureAwait(false);
            if (user == null)
            {
                user = new User
                {
                    DiscordUserID = discordUserId
                };
                entities.User.Add(user);
                await entities.SaveChangesAsync().ConfigureAwait(false);
            }
            return user.UserID;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IDatabaseAccess Members

        async Task IDatabaseAccess.AddUsers(IEnumerable<ulong> userIDs)
        {
            using (var entities = GetEntities())
            {
                var existingUserIDs = await entities.User.Select(m => m.DiscordUserID).ToArrayAsync().ConfigureAwait(false);
                var missingUserIDs = userIDs.Except(existingUserIDs.Select(m => (ulong)m)).ToArray();

                if (!missingUserIDs.Any())
                    return;

                _logger.LogInformation($"Adding {missingUserIDs.Length} missing users to the database...");
                var added = 0;
                foreach (var missingUserID in missingUserIDs)
                {
                    entities.User.Add(new User
                    {
                        DiscordUserID = missingUserID
                    });
                    added++;
                }

                await entities.SaveChangesAsync().ConfigureAwait(false);
                _logger.LogInformation($"Added {added} missing users to the database.");
            }
        }

        async Task<bool> IDatabaseAccess.AddUser(ulong userID)
        {
            using (var entities = GetEntities())
            {
                var decUserID = (decimal) userID;
                var userExists = await entities.User.AnyAsync(m => m.DiscordUserID == decUserID).ConfigureAwait(false);
                if (userExists)
                    return false;

                // Add missing user
                entities.User.Add(new User
                {
                    DiscordUserID = decUserID
                });

                await entities.SaveChangesAsync().ConfigureAwait(false);
                return true;
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

        async Task<bool> IDatabaseAccess.AddVacation(ulong userID, DateTime start, DateTime end, string note)
        {
            using (var entities = GetEntities())
            {
                var internalUserId = await GetInternalUserId(entities, userID).ConfigureAwait(false);

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

        async Task<bool> IDatabaseAccess.DeleteVacation(ulong userID, DateTime start, DateTime end)
        {
            using (var entities = GetEntities())
            {
                var internalUserId = await GetInternalUserId(entities, userID).ConfigureAwait(false);

                // Find the maching vacation
                var match = await entities.Vacation
                                          .SingleOrDefaultAsync(m => m.UserID == internalUserId
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

        async Task IDatabaseAccess.DeleteVacations(ulong userID)
        {
            using (var entities = GetEntities())
            {
                var internalUserId = await GetInternalUserId(entities, userID).ConfigureAwait(false);
                var vacations = await entities.Vacation.Where(m => m.UserID == internalUserId).ToArrayAsync().ConfigureAwait(false);
                if (!vacations.Any())
                    return;
                // If the user has any vacations in the database, delete them
                entities.Vacation.RemoveRange(vacations);
                await entities.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        async Task<(ulong UserID, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacations()
        {
            using (var entities = GetEntities())
            {
                var localItems = await entities.Vacation
                                               .Where(m => m.End >= DateTime.Today)
                                               .Join(entities.User, vacation => vacation.UserID, user => user.UserID, (vacation, user) => new {user.DiscordUserID, vacation.Start, vacation.End, vacation.Note})
                                               .ToArrayAsync()
                                               .ConfigureAwait(false);
                return localItems.Select(m => ((ulong)m.DiscordUserID, m.Start, m.End, m.Note)).ToArray();
            }
        }

        async Task<(ulong UserID, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacations(ulong userID)
        {
            using (var entities = GetEntities())
            {
                var localItems = await entities.Vacation
                                               .Where(m => m.End >= DateTime.Today)
                                               .Join(entities.User, vacation => vacation.UserID, user => user.UserID, (vacation, user) => new { user.DiscordUserID, vacation.Start, vacation.End, vacation.Note })
                                               .Where(m => m.DiscordUserID == userID)
                                               .ToArrayAsync()
                                               .ConfigureAwait(false);
                return localItems.Select(m => ((ulong)m.DiscordUserID, m.Start, m.End, m.Note)).ToArray();
            }
        }

        async Task<(ulong UserID, DateTime Start, DateTime End, string Note)[]> IDatabaseAccess.GetVacations(DateTime date)
        {
            using (var entities = GetEntities())
            {
                var localItems = await entities.Vacation
                                               .Where(m => date >= m.Start
                                                        && date <= m.End)
                                               .Join(entities.User, vacation => vacation.UserID, user => user.UserID, (vacation, user) => new { user.DiscordUserID, vacation.Start, vacation.End, vacation.Note })
                                               .ToArrayAsync()
                                               .ConfigureAwait(false);
                return localItems.Select(m => ((ulong)m.DiscordUserID, m.Start, m.End, m.Note)).ToArray();
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

        async Task IDatabaseAccess.UpdateUserInfoLastSeen(ulong userID, DateTime lastSeen)
        {
            using (var entities = GetEntities())
            {
                var internalUserID = await GetInternalUserId(entities, userID).ConfigureAwait(false);
                var existingInfo = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserID == internalUserID).ConfigureAwait(false);
                if (existingInfo != null)
                {
                    existingInfo.LastSeen = lastSeen;
                    await entities.SaveChangesAsync().ConfigureAwait(false);
                    return;
                }

                // Create new user info
                entities.UserInfo.Add(new UserInfo {UserID = internalUserID, LastSeen = lastSeen});
                await entities.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        async Task<(ulong UserID, DateTime? LastSeen)[]> IDatabaseAccess.GetLastSeenInfoForUsers(ulong[] userIDs)
        {
            if (userIDs == null)
                throw new ArgumentNullException(nameof(userIDs));
            if (userIDs.Length == 0)
                throw new ArgumentException("Parameter cannot be empty.", nameof(userIDs));

            var result = new List<(ulong UserID, DateTime? LastSeen)>();
            using (var entities = GetEntities())
            {
                foreach (var userID in userIDs)
                {
                    var internalUserID = await GetInternalUserId(entities, userID).ConfigureAwait(false);
                    var ui = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserID == internalUserID).ConfigureAwait(false);
                    result.Add((userID, ui?.LastSeen));
                }
            }

            return result.ToArray();
        }

        async Task IDatabaseAccess.DeleteUserInfo(ulong userID)
        {
            using (var entities = GetEntities())
            {
                var internalUserID = await GetInternalUserId(entities, userID).ConfigureAwait(false);
                var ui = await entities.UserInfo.SingleOrDefaultAsync(m => m.UserID == internalUserID).ConfigureAwait(false);
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