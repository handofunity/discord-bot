using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace HoU.GuildBot.BLL
{
    [UsedImplicitly]
    public class UnityHubSyncService
    {
        private readonly IDiscordAccess _discordAccess;
        private readonly IUnityHubAccess _unityHubAccess;
        private readonly AppSettings _appSettings;
        private readonly ILogger<UnityHubSyncService> _logger;

        public UnityHubSyncService(IDiscordAccess discordAccess,
                                   IUnityHubAccess unityHubAccess,
                                   AppSettings appSettings,
                                   ILogger<UnityHubSyncService> logger)
        {
            _discordAccess = discordAccess ?? throw new ArgumentNullException(nameof(discordAccess));
            _unityHubAccess = unityHubAccess ?? throw new ArgumentNullException(nameof(unityHubAccess));
            _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private void SanitizeUsers(IEnumerable<UserModel> users)
        {
            foreach (var userModel in users)
            {
                // Remove "a_" prefix of animated avatar IDs
                if (userModel.AvatarId != null && userModel.AvatarId.StartsWith("a_"))
                    userModel.AvatarId = userModel.AvatarId.Substring(2);
            }
        }

        public async Task SyncAllUsers()
        {
            if (string.IsNullOrWhiteSpace(_appSettings.UnityHubBaseAddress))
            {
                _logger.LogWarning("UnityHub base address not configured.");
                return;
            }

            if (string.IsNullOrWhiteSpace(_appSettings.UnityHubAccessSecret))
            {
                _logger.LogWarning("UnityHub access secret not configured.");
                return;
            }

            if (!_discordAccess.IsConnected || !_discordAccess.IsGuildAvailable)
                return;

            var allowedRoles = _unityHubAccess.GetValidRoleNames();
            var users = _discordAccess.GetUsersInRoles(allowedRoles);
            if (users.Any())
            {
                SanitizeUsers(users);
                var result = await _unityHubAccess.SendAllUsersAsync(users);
                if (result)
                {
                    _logger.LogInformation("Successfully synchronized users with the Unity Hub.");
                }
                else
                {
                    const string error = "Failed to synchronize users with the Unity Hub.";
                    _logger.LogError(error);
                    var leadershipMention = _discordAccess.GetLeadershipMention();
                    await _discordAccess.LogToDiscord($"**{leadershipMention} - {error}**");
                }
            }
            else
            {
                _logger.LogWarning("Failed to synchronize all users: {Reason}.", "unable to fetch allowed users");
            }
        }
    }
}