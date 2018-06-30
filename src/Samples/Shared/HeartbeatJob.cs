#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2018 Alphacloud.Net

#endregion

namespace SimpleService.Jobs
{
    using System;
    using System.Threading.Tasks;
    using AppServices;
    using Quartz;

    public class HeartbeatJob : IJob
    {
        private readonly IHeartbeatService _heartbeat;

        public HeartbeatJob(IHeartbeatService heartbeat)
        {
            _heartbeat = heartbeat ?? throw new ArgumentNullException(nameof(heartbeat));
        }

        public Task Execute(IJobExecutionContext context)
        {
            _heartbeat.UpdateServiceState("alive");
            return Task.CompletedTask;
        }
    }
}
