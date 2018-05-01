namespace HoU.GuildBot.BLL
{
    using System.Collections.Generic;
    using System.Linq;
    using JetBrains.Annotations;
    using Microsoft.Extensions.Logging;
    using Shared.BLL;
    using Shared.Enums;
    using Shared.Objects;

    [UsedImplicitly]
    public class CommandRegistry : ICommandRegistry
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Fields

        private readonly ILogger<CommandRegistry> _logger;
        private readonly IList<CommandInfo> _commands;

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
        #region ICommandRegistry Members

        void ICommandRegistry.RegisterAndValidateCommands(CommandInfo[] commands)
        {
            _commands.Clear();
            using (_logger.BeginScope("Command Validation"))
            {
                foreach (var ci in commands)
                {
                    _logger.LogDebug($"Validating command '{ci.Name}'...");

                    if (ci.AllowedRequestTypes == RequestType.Undefined)
                    {
                        _logger.LogDebug($"Didn't add command '{ci.Name}' because the allowed request types are undefined.");
                        continue;
                    }

                    if (ci.ResponseType == ResponseType.Undefined)
                    {
                        _logger.LogDebug($"Didn't add command '{ci.Name}' because the response type is undefined.");
                        continue;
                    }

                    if (ci.AllowedRoles == Role.NoRole)
                    {
                        _logger.LogDebug($"Didn't add command '{ci.Name}' because the allowed roles are undefined.");
                        continue;
                    }

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