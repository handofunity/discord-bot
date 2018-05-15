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

                _logger.LogInformation("Adding missing users to database...");
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

        #endregion
    }
}