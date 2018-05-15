namespace HoU.GuildBot.BLL
{
    using System;
    using System.Threading.Tasks;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.DAL;
    using Shared.Enums;
    using Shared.Objects;

    [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
    public class GuildUserPromoter : IGuildUserPromoter
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly IGuildUserRegistry _guildUserRegistry;
        private readonly IDiscordAccess _discordAccess;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public GuildUserPromoter(IGuildUserRegistry guildUserRegistry,
                                 IDiscordAccess discordAccess)
        {
            _guildUserRegistry = guildUserRegistry;
            _discordAccess = discordAccess;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IGuildUserPromoter Members

        async Task<GuildMemberPromotionResult> IGuildUserPromoter.TryPromote((ulong UserId, string Mention) promoter,
                                                                             (ulong UserId, string Mention) toBePromoted)
        {
            var promoterRoles = _guildUserRegistry.GetGuildUserRoles(promoter.UserId);
            var toBePromotedRoles = _guildUserRegistry.GetGuildUserRoles(toBePromoted.UserId);

            // Check if the person to be promoted can be promoted by the promoter
            if (promoterRoles.HasFlag(Role.Leader) && toBePromotedRoles.HasFlag(Role.Leader))
                return new GuildMemberPromotionResult($"{promoter.Mention}: A **{Role.Leader}** cannot promote a **{Role.Leader}**.");
            if (promoterRoles.HasFlag(Role.SeniorOfficer) && toBePromotedRoles.HasFlag(Role.SeniorOfficer))
                return new GuildMemberPromotionResult($"{promoter.Mention}: A **Senior Officer** cannot promote a **Senior Officer**.");
            if (promoterRoles.HasFlag(Role.SeniorOfficer) && toBePromotedRoles.HasFlag(Role.Leader))
                return new GuildMemberPromotionResult($"{promoter.Mention}: A **Senior Officer** cannot promote a **{Role.Leader}**.");
            if (promoterRoles.HasFlag(Role.Leader) && toBePromotedRoles.HasFlag(Role.SeniorOfficer))
                return new GuildMemberPromotionResult($"{promoter.Mention}: A **{Role.Leader}** cannot promote a **Senior Officer** any further with this command.");
            if (promoterRoles.HasFlag(Role.Leader) && toBePromotedRoles.HasFlag(Role.Member))
                return new GuildMemberPromotionResult($"{promoter.Mention}: A **{Role.Leader}** cannot promote a **{Role.Member}** any further with this command.");
            if (promoterRoles.HasFlag(Role.SeniorOfficer) && toBePromotedRoles.HasFlag(Role.Member))
                return new GuildMemberPromotionResult($"{promoter.Mention}: A **Senior Officer** cannot promote a **{Role.Member}** any further with this command.");

            // If the promotion is possible, calculate the new role and the role to remove
            var oldRole = Role.NoRole;
            var newRole = Role.NoRole;
            if (toBePromotedRoles.HasFlag(Role.Recruit))
            {
                oldRole = Role.Recruit;
                newRole = Role.Member;
            }
            else if (toBePromotedRoles.HasFlag(Role.Guest))
            {
                oldRole = Role.Guest;
                newRole = Role.Recruit;
            }
            else if (toBePromotedRoles.HasFlag(Role.NoRole))
            {
                oldRole = Role.NoRole;
                newRole = Role.Recruit;
            }

            if (newRole == Role.NoRole)
                return new GuildMemberPromotionResult($"{promoter.Mention}: Couldn't determine new role.");

            // Perform promotion
            // First, add new role - otherwise the user might get kicked out from current channels
            await _discordAccess.AssignRole(toBePromoted.UserId, newRole).ConfigureAwait(false);
            // Remove old role afterwards
            // If the old role is Role.NoRole, there's no role to revoke
            if (oldRole != Role.NoRole)
                await _discordAccess.RevokeRole(toBePromoted.UserId, oldRole).ConfigureAwait(false);

            // Return result for announcement and logging
            var description = $"Congratulations {toBePromoted.Mention}, you've been promoted to the rank **{newRole}**.";
            if (newRole == Role.Recruit)
                description += " Welcome aboard!";
            var a = new EmbedData
            {
                Title = "Promotion",
                Color = Colors.BrightBlue,
                Description = description
            };
            return new GuildMemberPromotionResult(a, $"{promoter.Mention} promoted {toBePromoted.Mention} from **{oldRole}** to **{newRole}**.");
        }

        #endregion
    }
}