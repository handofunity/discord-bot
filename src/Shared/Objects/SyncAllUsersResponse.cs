namespace HoU.GuildBot.Shared.Objects
{
    using System.Collections.Generic;

    public class SyncAllUsersResponse
    {
        public int CreatedUsers { get; set; }

        public int UpdatedUsers { get; set; }

        public int SkippedUsers { get; set; }

        public int UpdatedUserRoleRelations { get; set; }

        public List<string> Errors { get; set; }

        public SyncAllUsersResponse()
        {
            Errors = new List<string>();
        }
    }
}