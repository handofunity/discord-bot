namespace HoU.GuildBot.Shared.Objects;

[DebuggerDisplay(nameof(DisplayName))]
public class EventVoiceChannel
{
    [JsonProperty]
    public byte GroupNumber { get; }

    [JsonIgnore]
    public string DisplayName { get; }

    [JsonIgnore]
    public byte MaxUsersInChannel { get; }

    [JsonIgnore]
    public DiscordChannelId DiscordVoiceChannelIdValue { get; set; }

    [JsonProperty]
    public string DiscordVoiceChannelId => DiscordVoiceChannelIdValue.ToString();

    public EventVoiceChannel(int appointmentId)
    {
        GroupNumber = 0;
        DisplayName = $"General (UNITS #{appointmentId})";
        MaxUsersInChannel = 0;
    }

    public EventVoiceChannel(int appointmentId,
                             byte groupNumber,
                             byte maxUsersInChannel)
    {
        GroupNumber = groupNumber;
        DisplayName = $"#{groupNumber} | Group {GetGroupEmoji(groupNumber)} (UNITS #{appointmentId})";
        MaxUsersInChannel = maxUsersInChannel;
    }

    private static string GetGroupEmoji(int groupNumber)
    {
        return groupNumber switch
        {
            1 => "\uD83D\uDC31 Cat",
            2 => "\uD83E\uDD8D Gorilla",
            3 => "\uD83D\uDC14 Chicken",
            4 => "\uD83E\uDD96 T-Rex",
            5 => "\uD83D\uDC22 Turtle",
            6 => "\uD83D\uDC27 Penguin",
            7 => "\uD83E\uDD8A Fox",
            8 => "\uD83D\uDC36 Dog",
            9 => "\uD83D\uDC3C Panda",
            10 => "\uD83D\uDC09 Dragon",
            11 => "\uD83E\uDD81 Lion",
            12 => "\uD83E\uDD80 Crab",
            13 => "\uD83E\uDD8B Butterfly",
            14 => "\uD83E\uDD89 Owl",
            15 => "\uD83E\uDD88 Shark",
            16 => "\uD83D\uDC3A Wolf",
            17 => "\uD83E\uDDA2 Swan",
            18 => "\uD83D\uDC11 Sheep",
            19 => "\uD83E\uDD93 Zebra",
            20 => "\uD83D\uDC0C Snail",
            21 => "\uD83E\uDD94 Hedgehog",
            22 => "\uD83D\uDC2C Dolphin",
            23 => "\uD83D\uDC18 Elephant",
            24 => "\uD83E\uDDA9 Flamingo",
            25 => "\uD83E\uDD86 Duck",
            _ => throw new ArgumentOutOfRangeException(nameof(groupNumber))
        };
    }
}