namespace HoU.GuildBot.Shared.Attributes
{
    using System;
    using Enums;

    [AttributeUsage(AttributeTargets.Method)]
    public class ResponseContextAttribute : Attribute
    {
        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Properties

        public ResponseType ResponseType { get; }

        #endregion

        ////////////////////////////////////////////////////////////////////////////////////////////////////////
        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ResponseContextAttribute"/>.
        /// </summary>
        /// <param name="responseType">The desired response type of the command.</param>
        /// <exception cref="ArgumentException"><paramref name="responseType"/> equals <see cref="Enums.ResponseType.Undefined"/>.</exception>
        public ResponseContextAttribute(ResponseType responseType)
        {
            if (responseType == ResponseType.Undefined)
                throw new ArgumentOutOfRangeException(nameof(responseType), $"Value cannot be '{nameof(ResponseType.Undefined)}'.");
            ResponseType = responseType;
        }

        #endregion
    }
}