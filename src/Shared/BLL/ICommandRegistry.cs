using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.Shared.BLL
{
    public interface ICommandRegistry
    {
        bool CommandsRegistered { get; }

        void RegisterAndValidateCommands(CommandInfo[] commands);
        CommandInfo[] GetAvailableCommands(Role userRoles);
    }
}