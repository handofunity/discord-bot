using System;
using System.Collections.Generic;

namespace HoU.GuildBot.Shared.Objects
{
    public class SyncCreatedVoiceChannelsRequest
    {
        public int AppointmentId { get; }

        public List<EventVoiceChannel> CreatedVoiceChannels { get; }

        public SyncCreatedVoiceChannelsRequest(int appointmentId,
                                               List<EventVoiceChannel> createdVoiceChannels)
        {
            AppointmentId = appointmentId;
            CreatedVoiceChannels = createdVoiceChannels ?? throw new ArgumentNullException(nameof(createdVoiceChannels));
        }
    }
}