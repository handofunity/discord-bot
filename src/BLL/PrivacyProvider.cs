using System.Threading.Tasks;
using System.Timers;
using JetBrains.Annotations;
using HoU.GuildBot.Shared.BLL;
using HoU.GuildBot.Shared.DAL;
using HoU.GuildBot.Shared.Objects;

namespace HoU.GuildBot.BLL;

[UsedImplicitly]
public class PrivacyProvider : IPrivacyProvider
{
    private readonly IDatabaseAccess _databaseAccess;
    private readonly Timer _timer;
    
    public PrivacyProvider(IDatabaseAccess databaseAccess)
    {
        _databaseAccess = databaseAccess;
        _timer = new Timer
        {
            AutoReset = true,
            Interval = 86_400_000 // 1 day
        };
        _timer.Elapsed += Timer_Elapsed;
    }
    
    void IPrivacyProvider.Start()
    {
        _timer.Start();
    }

    async Task IPrivacyProvider.DeleteUserRelatedData(User user)
    {
        await _databaseAccess.DeleteUserInfoAsync(user);
        await _databaseAccess.DeleteVacationsAsync(user);
    }
    
    private void Timer_Elapsed(object? sender, ElapsedEventArgs e)
    {
        // Fire & forget
        Task.Run(() => _databaseAccess.DeletePastVacationsAsync());
    }
}