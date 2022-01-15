using System;
using Hangfire;

namespace HoU.GuildBot.Core;

internal class ContainerJobActivator : JobActivator
{
    private readonly IServiceProvider _serviceProvider;

    public ContainerJobActivator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
    }

    public override object? ActivateJob(Type jobType)
    {
        return _serviceProvider.GetService(jobType);
    }
}