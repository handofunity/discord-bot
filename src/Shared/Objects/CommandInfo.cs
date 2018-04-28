namespace HoU.GuildBot.Shared.Objects
{
    using Enums;

    public class CommandInfo
    {
        public string Name { get; }

        public string[] InvokeNames { get; }

        public RequestType AllowedRequestTypes { get; }

        public ResponseType ResponseType { get; }

        public Role AllowedRoles { get; }

        public string Summary { get; set; }

        public string Remarks { get; set; }

        public CommandInfo(string name, string[] invokeNames, RequestType allowedRequestTypes, ResponseType responseType, Role allowedRoles)
        {
            Name = name;
            InvokeNames = invokeNames;
            AllowedRequestTypes = allowedRequestTypes;
            ResponseType = responseType;
            AllowedRoles = allowedRoles;
        }
    }
}