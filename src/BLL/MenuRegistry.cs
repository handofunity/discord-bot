namespace HoU.GuildBot.BLL;

public class MenuRegistry : IMenuRegistry
{
    private readonly IGameRoleProvider _gameRoleProvider;
    private readonly INonMemberRoleProvider _nonMemberRoleProvider;
    private readonly IDynamicConfiguration _dynamicConfiguration;
    private readonly Dictionary<string, ButtonMenu> _buttonMenus;
    private readonly Dictionary<string, SelectMenu> _selectMenus;

    private IDiscordAccess? _discordAccess;

    public MenuRegistry(IGameRoleProvider gameRoleProvider,
                        INonMemberRoleProvider nonMemberRoleProvider,
                        IDynamicConfiguration dynamicConfiguration)
    {
        _gameRoleProvider = gameRoleProvider;
        _nonMemberRoleProvider = nonMemberRoleProvider;
        _dynamicConfiguration = dynamicConfiguration;

        _buttonMenus = new Dictionary<string, ButtonMenu>();
        _selectMenus = new Dictionary<string, SelectMenu>();
    }

    private static DiscordRoleId[] ToDiscordRoleIds(IReadOnlyCollection<string> values)
    {
        var result = new List<DiscordRoleId>(values.Count);
        foreach (var value in values)
            if (ulong.TryParse(value, out var ulongValue))
                result.Add((DiscordRoleId)ulongValue);
        return result.ToArray();
    }

    private bool IsRevokeModalResponse(ModalResponse response,
                                       out DiscordUserId userId,
                                       out string? parentSelectMenuCustomId,
                                       out IReadOnlyCollection<string>? selectedValues)
    {
        userId = response.UserId;
        
        if (IsRevokeModalId(response.CustomId, out parentSelectMenuCustomId))
        {
            selectedValues = response.Items.Single().Values;
            return true;
        }

        selectedValues = null;
        return false;

        bool IsRevokeModalId(string customId, out string? parentSelectMenuCustomId)
        {
            var isValidCustomId = customId.Length == 86 // 36 guid + 8 middle + 36 guid + 6 suffix
                && customId[36..44] == "_revoke_"   
                && customId[^6..] == "_modal"
                && _selectMenus.ContainsKey(customId[..36]);
            parentSelectMenuCustomId = isValidCustomId ? customId[..36] : null;
            return isValidCustomId;
        }
    }

    private bool IsRevokeSelectWorkaroundResponse(string customId,
                                                  out string? parentSelectMenuCustomId)
    {
        var isValidCustomId = customId.Length == 98 // 36 guid + 8 middle + 36 guid + 18 suffix
                           && customId[36..44] == "_revoke_"   
                           && customId[^18..] == "_select_workaround"
                           && _selectMenus.ContainsKey(customId[..36]);
        parentSelectMenuCustomId = isValidCustomId ? customId[..36] : null;
        return isValidCustomId;
    }

    private async Task<string?> PrimaryGameRoleCallback(DiscordUserId userId,
                                                        string customId,
                                                        IReadOnlyCollection<string> selectedValues,
                                                        RoleToggleMode roleToggleMode)
    {
        if (!Constants.Menus.IsMappedToPrimaryGameRoleIdConfigurationKey(customId, out var primaryGameRoleIdConfigurationKey))
            return string.Empty;
        
        // If the action is one of the primary game role actions, forward the data to the game role provider.
        var primaryGameDiscordRoleId = (DiscordRoleId)_dynamicConfiguration.DiscordMapping[primaryGameRoleIdConfigurationKey!];
        return await _gameRoleProvider.ToggleGameSpecificRolesAsync(userId,
                                                                    customId,
                                                                    _gameRoleProvider.Games.Single(m => m.PrimaryGameDiscordRoleId
                                                                     == primaryGameDiscordRoleId),
                                                                    selectedValues,
                                                                    roleToggleMode);

    }

    private SelectMenu CreatePrimaryGameRoleSelectMenu(string customId,
                                                       string placeholder,
                                                       IDictionary<string, string> options,
                                                       bool allowMultiple) =>
        new(customId,
            placeholder,
            options,
            allowMultiple,
            async (userId,
                   callbackCustomId,
                   selectedValues) =>
                await PrimaryGameRoleCallback(userId, callbackCustomId, selectedValues, RoleToggleMode.Assign),
            async (userId, responseCustomId, selectedValues) =>
            {
                // TODO: Replace temporary solution with modal.
                // if (IsRevokeModalResponse(response,
                //                           out var userId,
                //                           out var parentSelectMenuCustomId,
                //                           out var selectedValues))
                // {
                //     return await PrimaryGameRoleCallback(userId, parentSelectMenuCustomId!, selectedValues!, RoleToggleMode.Revoke);
                // }

                if (IsRevokeSelectWorkaroundResponse(responseCustomId,
                                                     out var parentSelectMenuCustomId))
                {
                    return await PrimaryGameRoleCallback(userId,
                                                         parentSelectMenuCustomId!,
                                                         selectedValues,
                                                         RoleToggleMode.Revoke);
                }

                return "Unexpected response.";
            });

    private void AddGameInterestMenuOptions(INonMemberRoleProvider nonMemberRoleProvider)
    {
        _selectMenus[Constants.GameInterestMenu.CustomId] = new SelectMenu(Constants.GameInterestMenu.CustomId,
                                                                           "Select game interests ...",
                                                                           GetCurrentOptions(),
                                                                           true,
                                                                           async (userId,
                                                                                  customId,
                                                                                  selectedValues) =>
                                                                               await nonMemberRoleProvider.ToggleNonMemberRoleAsync(userId,
                                                                                   customId,
                                                                                   ToDiscordRoleIds(selectedValues),
                                                                                   RoleToggleMode.Assign),
                                                                           async (userId, responseCustomId, selectedValues) =>
                                                                           {
                                                                               // TODO: Replace temporary solution with modal.
                                                                               // if (IsRevokeModalResponse(response,
                                                                               //         out var userId,
                                                                               //         out var parentSelectMenuCustomId,
                                                                               //         out var selectedValues))
                                                                               // {
                                                                               //     return await nonMemberRoleProvider
                                                                               //               .ToggleNonMemberRoleAsync(userId,
                                                                               //                    parentSelectMenuCustomId!,
                                                                               //                    ToDiscordRoleIds(selectedValues!),
                                                                               //                    RoleToggleMode.Revoke);
                                                                               // }
                                                                               if (IsRevokeSelectWorkaroundResponse(responseCustomId,
                                                                                       out var parentSelectMenuCustomId))
                                                                               {
                                                                                   return await nonMemberRoleProvider
                                                                                             .ToggleNonMemberRoleAsync(userId,
                                                                                                  parentSelectMenuCustomId!,
                                                                                                  ToDiscordRoleIds(selectedValues),
                                                                                                  RoleToggleMode.Revoke);
                                                                               }

                                                                               return "Unexpected response.";
                                                                           },
                                                                           existingOptions =>
                                                                           {
                                                                               var currentOptions = GetCurrentOptions();

                                                                               // Override existing options with current values.
                                                                               // Add new options at the same time.
                                                                               foreach (var currentOption in currentOptions)
                                                                                   existingOptions[currentOption.Key] = currentOption.Value;

                                                                               // Remove options that no longer exist.
                                                                               var optionsToRemove = existingOptions.Keys
                                                                                  .Except(currentOptions.Keys).ToArray();
                                                                               foreach (var optionToRemove in optionsToRemove)
                                                                                   existingOptions.Remove(optionToRemove);
                                                                           });

        IDictionary<string, string> GetCurrentOptions()
        {
            DiscordAccess.EnsureDisplayNamesAreSet(_gameRoleProvider.Games);
            return _gameRoleProvider.Games
                                     // Only those games with the GameInterestRoleId set can be used here
                                    .Where(m => m.GameInterestRoleId != null)
                                    .OrderBy(m => m.DisplayName)
                                    .Take(25)
                                    .ToDictionary(m => m.GameInterestRoleId.ToString()!, m => m.DisplayName ?? "<UNKNOWN>");
        }
    }

    private void AddGameRolesMenuOptions()
    {
        _selectMenus[Constants.GameRoleMenu.CustomId] = new SelectMenu(Constants.GameRoleMenu.CustomId,
                                                                       "Select games ...",
                                                                       GetCurrentOptions(),
                                                                       true,
                                                                       async (userId,
                                                                              _,
                                                                              values) => await Callback(userId,
                                                                                             values,
                                                                                             RoleToggleMode.Assign),
                                                                       async (userId, responseCustomId, selectedValues) =>
                                                                       {
                                                                           // TODO: Replace temporary solution with modal.
                                                                           // if (IsRevokeModalResponse(response,
                                                                           //         out var userId,
                                                                           //         out _,
                                                                           //         out var selectedValues))
                                                                           // {
                                                                           //     return await Callback(userId,
                                                                           //                selectedValues!,
                                                                           //                RoleToggleMode.Revoke);
                                                                           // }
                                                                           if (IsRevokeSelectWorkaroundResponse(responseCustomId, out _))
                                                                           {
                                                                               return await Callback(userId,
                                                                                          selectedValues,
                                                                                          RoleToggleMode.Revoke);
                                                                           }

                                                                           return "Unexpected response.";
                                                                       },
                                                                       existingOptions =>
                                                                       {
                                                                           var currentOptions = GetCurrentOptions();

                                                                           // Override existing options with current values.
                                                                           // Add new options at the same time.
                                                                           foreach (var currentOption in currentOptions)
                                                                               existingOptions[currentOption.Key] = currentOption.Value;

                                                                           // Remove options that no longer exist.
                                                                           var optionsToRemove = existingOptions.Keys
                                                                              .Except(currentOptions.Keys).ToArray();
                                                                           foreach (var optionToRemove in optionsToRemove)
                                                                               existingOptions.Remove(optionToRemove);
                                                                       });

        IDictionary<string, string> GetCurrentOptions()
        {
            DiscordAccess.EnsureDisplayNamesAreSet(_gameRoleProvider.Games);
            return _gameRoleProvider.Games
                                     // Only those games with the flag IncludeInGamesMenu enabled can be used here
                                    .Where(m => m.IncludeInGamesMenu)
                                    .OrderBy(m => m.DisplayName)
                                    .Take(100) // Can take 100 max, as this will result in 4 select components.
                                     // The fifth component is required for the revoke button. 
                                    .ToDictionary(m => m.PrimaryGameDiscordRoleId.ToString(), m => m.DisplayName ?? "<UNKNOWN>");
        }

        async Task<string?> Callback(DiscordUserId userId,
                                     IReadOnlyCollection<string> selectedValues,
                                     RoleToggleMode roleToggleMode)
        {
            var selectedGames = _gameRoleProvider.Games
                                                 .Where(m => selectedValues.Contains(m.PrimaryGameDiscordRoleId
                                                                                      .ToString()))
                                                 .ToArray();
            return await _gameRoleProvider
                      .TogglePrimaryGameRolesAsync(userId,
                                                   selectedGames,
                                                   roleToggleMode);
        }
    }

    public IDiscordAccess DiscordAccess
    {
        set => _discordAccess = value;
        private get => _discordAccess ?? throw new InvalidOperationException();
    }

    void IMenuRegistry.Initialize()
    {
        if (_buttonMenus.Any() ||_selectMenus.Any())
            return;

        _buttonMenus.Add(Constants.FriendOrGuestMenu.GuestCustomId,
                         new ButtonMenu(Constants.FriendOrGuestMenu.GuestCustomId,
                                        Constants.FriendOrGuestMenu.GuestDisplayName,
                                        async (userId,
                                               customId) =>
                                            await _nonMemberRoleProvider.ToggleNonMemberRoleAsync(userId,
                                                customId,
                                                Array.Empty<DiscordRoleId>(),
                                                RoleToggleMode.Dynamic)));
        _buttonMenus.Add(Constants.FriendOrGuestMenu.FriendOfMemberCustomId, new ButtonMenu(Constants.FriendOrGuestMenu
                                                                                               .FriendOfMemberCustomId,
                                                                                            Constants.FriendOrGuestMenu
                                                                                               .FriendofMemberDisplayName,
                                                                                            async (userId,
                                                                                                    customId) =>
                                                                                                await _nonMemberRoleProvider
                                                                                                   .ToggleNonMemberRoleAsync(userId,
                                                                                                        customId,
                                                                                                        Array.Empty<DiscordRoleId>(),
                                                                                                        RoleToggleMode.Dynamic)));

        _selectMenus.Add(Constants.AocArchetypeMenu.CustomId,
                         CreatePrimaryGameRoleSelectMenu(Constants.AocArchetypeMenu.CustomId,
                                                         "Select archetypes ...",
                                                         Constants.AocArchetypeMenu.GetOptions(),
                                                         true));
        _selectMenus.Add(Constants.AocPlayStyleMenu.CustomId,
                         CreatePrimaryGameRoleSelectMenu(Constants.AocPlayStyleMenu.CustomId,
                                                         "Select play styles ...",
                                                         Constants.AocPlayStyleMenu.GetOptions(),
                                                         true));
        _selectMenus.Add(Constants.AocRaceMenu.CustomId,
                         CreatePrimaryGameRoleSelectMenu(Constants.AocRaceMenu.CustomId,
                                                         "Select races ...",
                                                         Constants.AocRaceMenu.GetOptions(),
                                                         true));
        _selectMenus.Add(Constants.AocGuildPreferenceMenu.CustomId,
                         CreatePrimaryGameRoleSelectMenu(Constants.AocGuildPreferenceMenu.CustomId,
                                                         "Select preferred in-game guild ...",
                                                         Constants.AocGuildPreferenceMenu.GetOptions(),
                                                         false));
        _selectMenus.Add(Constants.AocRolePreferenceMenu.CustomId,
                         CreatePrimaryGameRoleSelectMenu(Constants.AocRolePreferenceMenu.CustomId,
                                                         "Select preferred role ...",
                                                         Constants.AocRolePreferenceMenu.GetOptions(),
                                                         false));
        _selectMenus.Add(Constants.WowClassMenu.CustomId,
                         CreatePrimaryGameRoleSelectMenu(Constants.WowClassMenu.CustomId,
                                                         "Select classes ...",
                                                         Constants.WowClassMenu.GetOptions(),
                                                         true));
        _selectMenus.Add(Constants.WowRetailPlayStyleMenu.CustomId,
                         CreatePrimaryGameRoleSelectMenu(Constants.WowRetailPlayStyleMenu.CustomId,
                                                         "Select play styles ...",
                                                         Constants.WowRetailPlayStyleMenu.GetOptions(),
                                                         true));
        _selectMenus.Add(Constants.LostArkPlayStyleMenu.CustomId,
                         CreatePrimaryGameRoleSelectMenu(Constants.LostArkPlayStyleMenu.CustomId,
                                                         "Select play styles ...",
                                                         Constants.LostArkPlayStyleMenu.GetOptions(),
                                                         true));
        _selectMenus.Add(Constants.TnlRolePreferenceMenu.CustomId,
                         CreatePrimaryGameRoleSelectMenu(Constants.TnlRolePreferenceMenu.CustomId,
                                                         "Select preferred role ...",
                                                         Constants.TnlRolePreferenceMenu.GetOptions(),
                                                         true));
        _selectMenus.Add(Constants.TnlWeaponMenu.CustomId,
                         CreatePrimaryGameRoleSelectMenu(Constants.TnlWeaponMenu.CustomId,
                                                         "Select weapons ...",
                                                         Constants.TnlWeaponMenu.GetOptions(),
                                                         true));
        
        AddGameInterestMenuOptions(_nonMemberRoleProvider);
        AddGameRolesMenuOptions();
    }

    bool IMenuRegistry.IsButtonMenu(string customId,
                                    out IMenuRegistry.ButtonCallback? callback)
    {
        if (_buttonMenus.TryGetValue(customId, out var buttonMenu))
        {
            callback = buttonMenu.Callback;
            return true;
        }

        callback = null;
        return false;
    }

    ButtonComponent IMenuRegistry.GetButtonMenuComponent(string customId) => _buttonMenus[customId].GetComponent();

    bool IMenuRegistry.IsRevokeButtonMenu(string customId)
    {
        return customId.Length == 49 // 36 guid + 13 suffix
            && customId[^13..] == "_revoke_start"
            && _selectMenus.ContainsKey(customId[..36]);
    }

    ModalData? IMenuRegistry.GetRevokeMenuModal(string customId,
                                                DiscordUserId userId)
    {
        if (!Constants.Menus.IsMappedToPrimaryGameRoleIdConfigurationKey(customId, out var primaryGameRoleIdConfigurationKey))
            return null;
        
        // Get role ids for available options.
        var options = _selectMenus[customId].Options;
        var primaryGameDiscordRoleId = (DiscordRoleId)_dynamicConfiguration.DiscordMapping[primaryGameRoleIdConfigurationKey!];
        var game = _gameRoleProvider.Games.Single(m => m.PrimaryGameDiscordRoleId == primaryGameDiscordRoleId);
        var availableOptions = options.Select<KeyValuePair<string, string>,
                                           (DiscordRoleId RoleId, string MenuValue, string MenuDisplayName)>(option => _dynamicConfiguration
                                          .DiscordMapping
                                          .TryGetValue($"{customId}___{option.Key}",
                                                       out var roleId)
                                           ? ((DiscordRoleId)roleId, option.Key, option.Value)
                                           : default)
                                      .Where(m => m != default && game.AvailableRoles.Any(r => r.DiscordRoleId == m.RoleId))
                                      .ToArray();
        
        // Filter for roles that are valid for the customId and the user currently has.
        var currentUserRoleIds = DiscordAccess.GetUserRoles(userId);
        var rolesThatCanBeRevoked = availableOptions.Where(m => currentUserRoleIds.Contains(m.RoleId))
                                                    .ToDictionary(m => m.MenuValue,
                                                                  m => m.MenuDisplayName);
        
        var revokeId = Guid.NewGuid().ToString("D");
        return new ModalData($"{customId}_revoke_{revokeId}_modal",
                             "Select role(s) to revoke",
                             new ActionComponent[]
                             {
                                 new SelectMenuComponent($"{customId}_revoke_{revokeId}_select",
                                                         0,
                                                         "Select role(s) to revoke ...",
                                                         rolesThatCanBeRevoked,
                                                         true)
                             });
    }

    SelectMenuComponent? IMenuRegistry.GetRevokeMenuSelectWorkaround(string customId,
                                                                     DiscordUserId userId)
    {
        if (!_selectMenus.TryGetValue(customId, out var selectMenu))
            return null;

        (DiscordRoleId RoleId, string MenuValue, string MenuDisplayName)[] availableOptions;
        if (Constants.Menus.IsMappedToPrimaryGameRoleIdConfigurationKey(customId, out var primaryGameRoleIdConfigurationKey))
        {
            // Map available options to valid game roles.
            var primaryGameDiscordRoleId = (DiscordRoleId)_dynamicConfiguration.DiscordMapping[primaryGameRoleIdConfigurationKey!];
            var game = _gameRoleProvider.Games.Single(m => m.PrimaryGameDiscordRoleId == primaryGameDiscordRoleId);
            availableOptions = selectMenu.Options.Select<KeyValuePair<string, string>,
                                              (DiscordRoleId RoleId, string MenuValue, string MenuDisplayName)>(option =>
                                              _dynamicConfiguration
                                                 .DiscordMapping
                                                 .TryGetValue($"{customId}___{option.Key}",
                                                              out var roleId)
                                                  ? ((DiscordRoleId)roleId, option.Key, option.Value)
                                                  : default)
                                         .Where(m => m != default && game.AvailableRoles.Any(r => r.DiscordRoleId == m.RoleId))
                                         .ToArray();
        }
        else
        {
            // Use all available options.
            availableOptions = selectMenu.Options
                                         .Select(m => (ulong.TryParse(m.Key, out var ulongRoleId)
                                                           ? (DiscordRoleId)ulongRoleId
                                                           : default,
                                                       m.Key,
                                                       m.Value))
                                         .Where(m => m.Item1 != default)
                                         .ToArray();
        }
        
        // Filter for roles that are valid for the customId and the user currently has.
        var currentUserRoleIds = DiscordAccess.GetUserRoles(userId);
        var rolesThatCanBeRevoked = availableOptions.Where(m => currentUserRoleIds.Contains(m.RoleId))
                                                    .ToDictionary(m => m.MenuValue,
                                                                  m => m.MenuDisplayName);
        if (rolesThatCanBeRevoked.Count == 0)
            return null;

        var revokeId = Guid.NewGuid().ToString("D");
        return new SelectMenuComponent($"{customId}_revoke_{revokeId}_select_workaround",
                                       0,
                                       "Select role(s) to revoke",
                                       rolesThatCanBeRevoked,
                                       true);
    }

    bool IMenuRegistry.IsSelectMenu(string customId,
                                    out IMenuRegistry.MenuCallback? callback)
    {
        if (_selectMenus.TryGetValue(customId, out var selectMenu))
        {
            callback = selectMenu.Callback;
            return true;
        }

        var selectMenuByPartialCustomId = _selectMenus.Values.SingleOrDefault(m => m.ContainsPartialCustomId(customId));
        if (selectMenuByPartialCustomId is not null)
        {
            callback = selectMenuByPartialCustomId.Callback;
            return true;
        }

        if (IsRevokeSelectWorkaroundResponse(customId, out var parentSelectMenuCustomId)
         && _selectMenus.TryGetValue(parentSelectMenuCustomId!, out var revokeSelectMenu))
        {
            callback = revokeSelectMenu.RevokeCallback;
            return true;
        }

        callback = null;
        return false;
    }

    IEnumerable<ActionComponent> IMenuRegistry.GetSelectMenuComponents(string customId)
    {
        if (!_selectMenus.TryGetValue(customId, out var selectMenu))
            throw new ArgumentOutOfRangeException(nameof(customId));

        foreach (var selectionComponent in selectMenu.GetSelectionComponents())
            yield return selectionComponent;
        yield return selectMenu.GetRevokeComponent();
    }

    bool IMenuRegistry.IsModalMenu(string customId,
                                   out IMenuRegistry.ModalCallback? callback)
    {
        // TODO: Replace temporary solution with modal.
        // if (_selectMenus.TryGetValue(customId, out var selectMenu))
        // {
        //     callback = selectMenu.RevokeCallback;
        //     return true;
        // }

        callback = null;
        return false;
    }

    void IMenuRegistry.UpdateExistingPartialCustomIds(string customId,
                                                      string[] existingPartialCustomIds)
    {
        if (_selectMenus.TryGetValue(customId, out var selectMenu))
            selectMenu.PartialCustomIds = existingPartialCustomIds;

    }

    private record ButtonMenu(string CustomId,
                              string DisplayName,
                              IMenuRegistry.ButtonCallback Callback)
    {
        internal ButtonComponent GetComponent() =>
            new(CustomId,
                0,
                DisplayName,
                InteractionButtonStyle.Primary);
    }

    private class SelectMenu
    {
        private readonly Action<IDictionary<string, string>>? _optionsUpdater;

        internal string CustomId { get; }

        internal string Placeholder { get; }

        internal IDictionary<string, string> Options { get; }

        internal bool AllowMultiple { get; }

        internal IMenuRegistry.MenuCallback Callback { get; }
        
        internal IMenuRegistry.MenuCallback RevokeCallback { get; }

        internal string[]? PartialCustomIds { get; set; }

        internal SelectMenu(string customId,
                            string placeholder,
                            IDictionary<string, string> options,
                            bool allowMultiple,
                            IMenuRegistry.MenuCallback callback,
                            IMenuRegistry.MenuCallback revokeCallback)
        {
            CustomId = customId;
            Placeholder = placeholder;
            Options = options;
            AllowMultiple = allowMultiple;
            Callback = callback;
            RevokeCallback = revokeCallback;
        }

        internal SelectMenu(string customId,
                            string placeholder,
                            IDictionary<string, string> options,
                            bool allowMultiple,
                            IMenuRegistry.MenuCallback callback,
                            IMenuRegistry.MenuCallback revokeCallback,
                            Action<IDictionary<string, string>> optionsUpdater)
            : this(customId, placeholder, options, allowMultiple, callback, revokeCallback)
        {
            _optionsUpdater = optionsUpdater;
        }

        internal bool ContainsPartialCustomId(string customId) => PartialCustomIds is not null && PartialCustomIds.Contains(customId);

        internal IEnumerable<SelectMenuComponent> GetSelectionComponents()
        {
            _optionsUpdater?.Invoke(Options);

            if (Options.Count > 25)
            {
                PartialCustomIds = Enumerable.Range(0, (int)Math.Ceiling(Options.Count / 25m))
                                             .Select(_ => Guid.NewGuid().ToString("D"))
                                             .ToArray();

                byte actionRowNumber = 0;

                foreach (var partialCustomId in PartialCustomIds)
                {
                    yield return new SelectMenuComponent(partialCustomId,
                                                         actionRowNumber,
                                                         "Select games ...",
                                                         Options.Skip(actionRowNumber * 25)
                                                                .Take(25)
                                                                .ToDictionary(m => m.Key, m => m.Value),
                                                         true);
                    actionRowNumber++;
                }
            }
            else
            {
                yield return new SelectMenuComponent(CustomId,
                                                     0,
                                                     Placeholder,
                                                     Options,
                                                     AllowMultiple);
            }
        }

        internal ButtonComponent GetRevokeComponent() =>
            new($"{CustomId}_revoke_start",
                PartialCustomIds is null
                    ? (byte)1
                    : (byte)PartialCustomIds.Length,
                "Click here to remove roles",
                InteractionButtonStyle.Danger);
    }
}