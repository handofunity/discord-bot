using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Interactions;
using JetBrains.Annotations;
using HoU.GuildBot.DAL.Discord.Preconditions;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.Enums;
using HoU.GuildBot.Shared.StrongTypes;

namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("user-info", "Commands related to querying and setting user related info.")]
public class UserInfoModule : InteractionModuleBase<SocketInteractionContext>
{
    private readonly IUserInfoProvider _userInfoProvider;
    private readonly IGuildInfoProvider _guildInfoProvider;

    public UserInfoModule(IUserInfoProvider userInfoProvider,
                          IGuildInfoProvider guildInfoProvider)
    {
        _userInfoProvider = userInfoProvider;
        _guildInfoProvider = guildInfoProvider;
    }

    [SlashCommand("who-is", "Gets internal information about the given user.")]
    [AllowedRoles(Role.Developer)]
    public async Task WhoIsAsync(IUser user)
    {
        var data = _userInfoProvider.WhoIs((DiscordUserId)user.Id);
        var embed = data.ToEmbed();
        await RespondAsync(embed: embed);
    }

    [SlashCommand("who-is-internal", "Gets internal information about the invoking or given user.")]
    [AllowedRoles(Role.Developer)]
    public async Task WhoIsAsync([MinValue(1)] int internalUserId)
    {
        var data = _userInfoProvider.WhoIs((InternalUserId)internalUserId);
        var embed = data.ToEmbed();
        await RespondAsync(embed: embed);
    }

    [SlashCommand("last-seen", "Lists all users and the timestamp of their last text message.", runMode: RunMode.Async)]
    [AllowedRoles(Role.Leader | Role.Officer)]
    public async Task LastSeenAsync()
    {
        await DeferAsync();

        var data = await _userInfoProvider.GetLastSeenInfo();

        var buffer = new StringBuilder();

        foreach (var s in data)
        {
            if (buffer.Length + s.Length < 2000)
            {
                buffer.AppendLine(s);
            }
            else
            {
                // Flush buffer
                await ReplyAsync(buffer.ToString());
                buffer.Clear();
                buffer.AppendLine(s);
            }
        }

        if (buffer.Length > 0)
            await ReplyAsync(buffer.ToString());

        await FollowupAsync("Information provided below.");
    }

    [SlashCommand("get-count", "Gets the count of guild members.")]
    [AllowedRoles(Role.AnyGuildMember)]
    public async Task GuildMembersAsync()
    {
        var data = _guildInfoProvider.GetGuildMemberStatus();
        var embed = data.ToEmbed();
        await RespondAsync(embed: embed);
    }
}