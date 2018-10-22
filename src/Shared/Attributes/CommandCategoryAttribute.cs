namespace HoU.GuildBot.Shared.Attributes
{
    using System;
    using Enums;

    [AttributeUsage(AttributeTargets.Method)]
    public class CommandCategoryAttribute : Attribute
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Properties

        public CommandCategory Category { get; }

        public uint Order { get; }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        /// <summary>
        /// Creates a new instance of the <see cref="CommandCategoryAttribute"/>.
        /// </summary>
        /// <param name="category">The <see cref="CommandCategory"/> to assign the command to.</param>
        /// <param name="order">The order of the command inside the <see cref="CommandCategory"/>.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="category"/> equals <see cref="CommandCategory.Undefined"/> or <paramref name="order"/> equals 0.</exception>
        public CommandCategoryAttribute(CommandCategory category,
                                        uint order)
        {
            if (category == CommandCategory.Undefined)
                throw new ArgumentOutOfRangeException(nameof(category), $"Value cannot be '{nameof(CommandCategory.Undefined)}'.");
            if (order == 0)
                throw new ArgumentOutOfRangeException(nameof(order), "Value cannot be '0'.");
            Category = category;
            Order = order;
        }

        #endregion
    }
}