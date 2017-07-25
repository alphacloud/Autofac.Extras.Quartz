#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014 Alphacloud.Net

#endregion

namespace SimpleService.Jobs
{
    using System;
    using AppServices;
    using Quartz;

    public class HeartbeatJob : IJob
    {
        private readonly IHeartbeatService _heartbeat;

        public HeartbeatJob(IHeartbeatService heartbeat)
        {
            _heartbeat = heartbeat ?? throw new ArgumentNullException(nameof(heartbeat));
        }

        public void Execute(IJobExecutionContext context)
        {
            _heartbeat.UpdateServiceState("alive");
        }
    }
}
