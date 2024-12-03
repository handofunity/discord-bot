namespace HoU.GuildBot.Shared.Objects;

public class SyncCurrentAttendeesRequest
{
    public int AppointmentId { get; }

    public int CheckNumber { get; }

    public List<ulong> UserIds { get; }

    public SyncCurrentAttendeesRequest(int appointmentId,
                                       int checkNumber,
                                       List<ulong> userIds)
    {
        AppointmentId = appointmentId;
        CheckNumber = checkNumber;
        UserIds = userIds;
    }
}