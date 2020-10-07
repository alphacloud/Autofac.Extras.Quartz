#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2019 Alphacloud.Net

#endregion

namespace SimpleService.AppServices
{
    using JetBrains.Annotations;
    using Serilog;

    internal class HeartbeatService : IHeartbeatService
    {
        [NotNull] private static readonly ILogger s_log = Log.ForContext<HeartbeatService>();

        public void UpdateServiceState(string state)
        {
            s_log.Information("Service state: {ServiceState}.", state);
        }
    }

    public interface IHeartbeatService
    {
        void UpdateServiceState(string state);
    }
}
