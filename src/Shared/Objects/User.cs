using System;
using System.Threading;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.Extensions;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.Shared.Objects;

public class User
{
    public static readonly DateTime DefaultJoinedDate = new(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private int _roles;
    
    public InternalUserId InternalUserId { get; }

    public DiscordUserId DiscordUserId { get; }

    public string Mention => DiscordUserId.ToMention();

    public DateTime JoinedDate { get; set; }

    public string? CurrentRoles { get; set; }

    public Role Roles
    {
        get => (Role)_roles;
        set => Interlocked.Exchange(ref _roles, (int)value);
    }

    public bool IsGuildMember => (Role.AnyGuildMember & Roles) != Role.NoRole;
    
    public User(InternalUserId internalUserID, DiscordUserId discordUserID)
    {
        InternalUserId = internalUserID;
        DiscordUserId = discordUserID;
        JoinedDate = DefaultJoinedDate;
    }
}