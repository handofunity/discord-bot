using Discord.Interactions;
using JetBrains.Annotations;

namespace HoU.GuildBot.DAL.Discord.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
[Group("config", "Commands for live bot configuration.")]
public partial class ConfigModule : InteractionModuleBase<SocketInteractionContext>
{

}