namespace HoU.GuildBot.Shared.Objects
{
    using System;
    using System.Collections.Generic;
    using JetBrains.Annotations;
    using StrongTypes;

    public class UserModel
    {
        [NotNull] private string _username;
        [NotNull] private IReadOnlyList<string> _roles;

        /// <summary>
        /// Gets or sets the user's Discord ID.
        /// </summary>
        public DiscordUserID DiscordUserId { get; set; }

        /// <summary>
        /// Gets or sets the user's Discord username, not unique across the platform.
        /// </summary>
        /// <exception cref="ArgumentNullException">The property gets set to <b>null</b>.</exception>
        public string Username
        {
            get => _username;
            set => _username = value ?? throw new ArgumentNullException(nameof(value), $"{nameof(Username)} cannot be set to null.");
        }

        /// <summary>
        /// Gets or sets the user's 4-digit Discord tag.
        /// </summary>
        public short Discriminator { get; set; }

        /// <summary>
        /// Gets or sets the user's Discord nickname on the guild's server.
        /// </summary>
        public string Nickname { get; set; }

        /// <summary>
        /// Gets or sets the user's Discord avatar hash.
        /// </summary>
        public string AvatarId { get; set; }

        /// <summary>
        /// Gets or sets the roles the user has.
        /// </summary>
        /// <exception cref="ArgumentNullException">The property gets set to <b>null</b>.</exception>
        [NotNull]
        public IReadOnlyList<string> Roles
        {
            get => _roles;
            set => _roles = value ?? throw new ArgumentNullException(nameof(value), $"{nameof(Roles)} cannot be set to null.");
        }

        /// <summary>
        /// Initializes a new <see cref="UserModel"/> instance, setting <see cref="Username"/> to <see cref="string.Empty"/>.
        /// </summary>
        public UserModel()
        {
            _username = string.Empty;
            _roles = new string[0];
        }
    }
}