namespace HoU.GuildBot.BLL
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using JetBrains.Annotations;
    using Shared.BLL;
    using Shared.Enums;
    using Shared.Extensions;
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

        private (string Message, EmbedData Embed)[] ListAvailableCommands(DiscordUserID userId)
        {
            var availableCommands = GetAvailableCommands(userId);
            if (availableCommands.Length == 0)
            {
                return new (string Message, EmbedData Embed)[]
                {
                    (string.Empty, new EmbedData
                        {
                            Title = "No commands available",
                            Color = Colors.LightOrange,
                            Description = "No commands are available for you."
                        })
                };
            }

            var header = "Available commands (name and shortcuts)." +
                         $"{Environment.NewLine}The list below shows all commands that are available for you. " +
                         $"{Environment.NewLine}If you need advanced help for a command, " +
                         "supply either a _command name_ or _command shortcut_ **in double quotation marks** to get additional help for that command." +
                         $"{Environment.NewLine}For example, use `hou!help \"help\"`, to get advanced help for the \"help\" command.";

            var commandsCategories = availableCommands.Select(ci => new {ci.CommandCategory, ci.CommandOrder, ci.Name, Invokes = $"\"{string.Join("\", \"", ci.InvokeNames)}\""})
                                                      .GroupBy(m => m.CommandCategory)
                                                      .OrderBy(m => m.Key);
            var result = new List<(string Message, EmbedData Embed)>
            {
                (header, null)
            };
            foreach (var commandCategory in commandsCategories)
            {
                result.Add((string.Empty, new EmbedData
                               {
                                   Title = commandCategory.Key.GetDisplayName(),
                                   Color = Colors.LightGreen,
                                   Fields = commandCategory.OrderBy(m => m.CommandOrder).Select(m => new EmbedField(m.Name, m.Invokes, false)).ToArray()
                               }));
            }
            return result.ToArray();
        }

        private (string Message, EmbedData Embed)[] GetAdvancedCommandHelp(DiscordUserID userId, string helpRequest)
        {
            var syntax = new Regex("\"(?<requestedCommand>.+)\"");
            var match = syntax.Match(helpRequest);
            if (!match.Success)
                return new (string Message, EmbedData Embed)[]{("Incorrect command usage, please specify the command with **double quotation marks**: `hou!help \"help\"`", null)};
            var requestedCommand = match.Groups["requestedCommand"].Value;
            var availableCommands = GetAvailableCommands(userId);
            var matchingCommands = availableCommands.Where(m => m.Name == requestedCommand || m.InvokeNames.Any(i => i == requestedCommand)).ToArray();
            if (matchingCommands.Length == 0)
                return new(string Message, EmbedData Embed)[] { ($"Couldn't find a matching command for \"{requestedCommand}\"", null)};

            var result = new List<(string Message, EmbedData Embed)>();
            foreach (var matchingCommand in matchingCommands)
            {
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

                    r.Add(new EmbedField("Available to", matchingCommand.AllowedRoles.GetDisplayName(), true));
                    r.Add(new EmbedField("Available in", matchingCommand.AllowedRequestTypes.GetDisplayName(), true));
                    r.Add(new EmbedField("Will respond in", matchingCommand.ResponseType.GetDisplayName(), true));

                    return r.ToArray();
                }

                result.Add((string.Empty, new EmbedData
                               {
                                   Title = "Command help",
                                   Color = Colors.LightGreen,
                                   Fields = GetFields()
                               }));
            }

            return result.ToArray();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region IHelpProvider Members

        (string Message, EmbedData EmbedData)[] IHelpProvider.GetHelp(DiscordUserID userId, string helpRequest)
        {
            return helpRequest == null
                       ? ListAvailableCommands(userId)
                       : GetAdvancedCommandHelp(userId, helpRequest);
        }

        #endregion
    }
}