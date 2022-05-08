#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2022 Alphacloud.Net

#endregion

// ReSharper disable once CheckNamespace
namespace SimpleService.AppServices;

using Serilog;

public interface IHeartbeatService
{
    void UpdateServiceState(string state);
}

class HeartbeatService : IHeartbeatService
{
    static readonly ILogger s_log = Log.ForContext<HeartbeatService>();

    public void UpdateServiceState(string state)
    {
        s_log.Information("Service state: {ServiceState}", state);
    }
}