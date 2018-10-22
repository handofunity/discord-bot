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

        public CommandCategory CommandCategory { get; }

        public uint CommandOrder { get; }

        public string Summary { get; set; }

        public string Remarks { get; set; }

        public CommandInfo(string name,
                           string[] invokeNames,
                           RequestType allowedRequestTypes,
                           ResponseType responseType,
                           Role allowedRoles,
                           CommandCategory commandCategory,
                           uint commandOrder)
        {
            Name = name;
            InvokeNames = invokeNames;
            AllowedRequestTypes = allowedRequestTypes;
            ResponseType = responseType;
            AllowedRoles = allowedRoles;
            CommandCategory = commandCategory;
            CommandOrder = commandOrder;
        }
    }
}