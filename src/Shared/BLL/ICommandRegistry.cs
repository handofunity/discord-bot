namespace HoU.GuildBot.Shared.BLL
{
    using Enums;
    using Objects;

    public interface ICommandRegistry
    {
        bool CommandsRegistered { get; }

        void RegisterAndValidateCommands(CommandInfo[] commands);
        CommandInfo[] GetAvailableCommands(Role userRoles);
    }
}