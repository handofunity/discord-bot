namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.Enums;
    using Shared.Objects;
    using Shared.StrongTypes;

    [UsedImplicitly]
    public class HelpProvider : IHelpProvider
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ICommandRegistry _commandRegistry;
        private readonly IUserStore _userStore;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public HelpProvider(ICommandRegistry commandRegistry,
                            IUserStore userStore)
        {
            _commandRegistry = commandRegistry;
            _userStore = userStore;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private CommandInfo[] GetAvailableCommands(DiscordUserID userID)
        {
            if (!_userStore.TryGetUser(userID, out var user))
                return new CommandInfo[0];
            return user.Roles == Role.NoRole
                       ? new CommandInfo[0]
                       : _commandRegistry.GetAvailableCommands(user.Roles);
        }

        private (string Message, EmbedData Embed) ListAvailableCommands(DiscordUserID userId)
        {
            const string responseTitle = "Available commands";

            var availableCommands = GetAvailableCommands(userId);
            if (availableCommands.Length == 0)
            {
                return (string.Empty, new EmbedData
                {
                    Title = responseTitle,
                    Color = Colors.LightOrange,
                    Description = "No commands are available for you."
                });
            }

            var message = "The list below shows all commands that are available for you. " +
                          $"{Environment.NewLine}If you need advanced help for a command, " +
                          "supply either a _command name_ or _command shortcut_ **in double quotation marks** to get additional help for that command." +
                          $"{Environment.NewLine}For example, use `hou!help \"help\"`, to get advanced help for the \"help\" command.";

            var commands = availableCommands.Select(ci => $"```\"{ci.Name}\": \"{string.Join("\", \"", ci.InvokeNames)}\"```");
            return (message, new EmbedData
            {
                Title = responseTitle,
                Color = Colors.LightGreen,
                Description = string.Join(Environment.NewLine, commands)
            });
        }

        private (string Message, EmbedData Embed) GetAdvancedCommandHelp(DiscordUserID userId, string helpRequest)
        {
            var syntax = new Regex("\"(?<requestedCommand>.+)\"");
            var match = syntax.Match(helpRequest);
            if (!match.Success)
                return ("Incorrect command usage, please specify the command with **double quotation marks**: `hou!help \"help\"`", null);
            var requestedCommand = match.Groups["requestedCommand"].Value;
            var availableCommands = GetAvailableCommands(userId);
            var matchingCommand = availableCommands.SingleOrDefault(m => m.Name == requestedCommand || m.InvokeNames.Any(i => i == requestedCommand));
            if (matchingCommand == null)
                return ($"Couldn't find a matching command for \"{requestedCommand}\"", null);

            EmbedField[] GetFields()
            {
                var r = new List<EmbedField>(new[]
                {
                    new EmbedField("Name", matchingCommand.Name, false),
                    new EmbedField("Shortcuts", $"\"{string.Join("\", \"", matchingCommand.InvokeNames)}\"", false)
                });

                if (!string.IsNullOrWhiteSpace(matchingCommand.Summary))
                    r.Add(new EmbedField("Summary", matchingCommand.Summary, false));
                if (!string.IsNullOrWhiteSpace(matchingCommand.Remarks))
                    r.Add(new EmbedField("Remarks", matchingCommand.Remarks, false));

                r.Add(new EmbedField("Available to", matchingCommand.AllowedRoles, true));
                r.Add(new EmbedField("Available in", matchingCommand.AllowedRequestTypes, true));
                r.Add(new EmbedField("Will respond in", matchingCommand.ResponseType, true));

                return r.ToArray();
            }
            return (string.Empty, new EmbedData
                       {
                           Title = "Command help",
                           Color = Colors.LightGreen,
                           Fields = GetFields()
                       });
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IHelpProvider Members

        (string Message, EmbedData EmbedData) IHelpProvider.GetHelp(DiscordUserID userId, string helpRequest)
        {
            return helpRequest == null
                       ? ListAvailableCommands(userId)
                       : GetAdvancedCommandHelp(userId, helpRequest);
        }

        #endregion
    }
}