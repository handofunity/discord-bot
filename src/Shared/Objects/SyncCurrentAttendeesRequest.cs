using System.Collections.Generic;

namespace HoU.GuildBot.Shared.Objects
{
    public class SyncCurrentAttendeesRequest
    {
        public int AppointmentId { get; }

        public int CheckNumber { get; }

        public List<VoiceChannelAttendees> VoiceChannels { get; }

        public SyncCurrentAttendeesRequest(int appointmentId,
                                           int checkNumber,
                                           List<VoiceChannelAttendees> voiceChannels)
        {
            AppointmentId = appointmentId;
            CheckNumber = checkNumber;
            VoiceChannels = voiceChannels;
        }
    }
}