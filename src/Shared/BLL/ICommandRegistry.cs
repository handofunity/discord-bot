namespace HoU.GuildBot.Shared.BLL
{
    using Enums;
    using Objects;

    public interface ICommandRegistry
    {
        void RegisterAndValidateCommands(CommandInfo[] commands);
        CommandInfo[] GetAvailableCommands(Role userRoles);
    }
}