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

        public ResponseContextAttribute(ResponseType responseType)
        {
            ResponseType = responseType;
        }

        #endregion
    }
}