#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2022 Alphacloud.Net

#endregion

// ReSharper disable once CheckNamespace
namespace SimpleService.Jobs;

using AppServices;

public class HeartbeatJob : IJob
{
    readonly IHeartbeatService _heartbeat;
    readonly IScopedDependency _scopedDependency;

    public HeartbeatJob(IHeartbeatService heartbeat, IScopedDependency scopedDependency)
    {
        _heartbeat = heartbeat ?? throw new ArgumentNullException(nameof(heartbeat));
        _scopedDependency = scopedDependency ?? throw new ArgumentNullException(nameof(scopedDependency));
    }

    public Task Execute(IJobExecutionContext context)
    {
        _heartbeat.UpdateServiceState($"alive, scope: {_scopedDependency.Scope}");
        return Task.CompletedTask;
    }
}
