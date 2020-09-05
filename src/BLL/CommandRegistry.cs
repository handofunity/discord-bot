using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL
{
    [UsedImplicitly]
    public class CommandRegistry : ICommandRegistry
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<CommandRegistry> _logger;
        private readonly IList<CommandInfo> _commands;
        private bool _commandsRegistered;

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        public CommandRegistry(ILogger<CommandRegistry> logger)
        {
            _logger = logger;
            _commands = new List<CommandInfo>();
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Private Methods

        private bool IsCommandValid(CommandInfo ci)
        {
            _logger.LogDebug($"Validating command '{ci.Name}'...");

            if (ci.AllowedRequestTypes == RequestType.Undefined)
            {
                _logger.LogDebug($"Didn't add command '{ci.Name}' because the allowed request types are undefined.");
                return false;
            }

            if (ci.ResponseType == ResponseType.Undefined)
            {
                _logger.LogDebug($"Didn't add command '{ci.Name}' because the response type is undefined.");
                return false;
            }

            if (ci.AllowedRoles == Role.NoRole)
            {
                _logger.LogDebug($"Didn't add command '{ci.Name}' because the allowed roles are undefined.");
                return false;
            }

            if (ci.CommandCategory == CommandCategory.Undefined)
            {
                _logger.LogDebug($"Didn't add command '{ci.Name}' because the command category is not defined.");
                return false;
            }

            if (ci.CommandOrder == 0)
            {
                _logger.LogDebug($"Didn't add command '{ci.Name}' because the command order is not defined.");
                return false;
            }

            return true;
        }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region ICommandRegistry Members

        bool ICommandRegistry.CommandsRegistered => _commandsRegistered;

        void ICommandRegistry.RegisterAndValidateCommands(CommandInfo[] commands)
        {
            _commandsRegistered = true;
            _commands.Clear();
            using (_logger.BeginScope("Command Validation"))
            {
                foreach (var ci in commands)
                {
                    if (!IsCommandValid(ci)) continue;

                    _commands.Add(ci);
                    _logger.LogDebug($"Added command '{ci.Name}' to registry.");
                }

                _logger.LogInformation($"{_commands.Count} commands have been added successfully.");
                if (_commands.Count != commands.Length)
                    _logger.LogWarning($"{commands.Length - _commands.Count} commands were not added.");
            }
        }

        CommandInfo[] ICommandRegistry.GetAvailableCommands(Role userRoles)
        {
            return _commands.Where(m => (m.AllowedRoles & userRoles) != Role.NoRole).ToArray();
        }

        #endregion
    }
}