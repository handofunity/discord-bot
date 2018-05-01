namespace HoU.GuildBot.Shared.BLL
{
    using System.Threading.Tasks;
    using Objects;

    public interface IGuildUserPromoter
    {
        Task<GuildMemberPromotionResult> TryPromote((ulong UserId, string Mention) promoter, (ulong UserId, string Mention) toBePromoted);
    }
}