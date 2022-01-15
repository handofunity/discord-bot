using System.Threading.Tasks;

namespace HoU.GuildBot.Shared.BLL;

public interface IRoleRemover
{
    Task RemoveBasementRolesAsync();
}