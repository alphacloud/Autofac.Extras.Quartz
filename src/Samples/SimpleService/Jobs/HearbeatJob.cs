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
    using Common.Logging;
    using Quartz;

    public class HearbeatJob : IJob
    {
        private readonly IHeartbeatService _hearbeat;
        private static readonly ILog s_log = LogManager.GetCurrentClassLogger();

        public HearbeatJob(IHeartbeatService hearbeat)
        {
            if (hearbeat == null) throw new ArgumentNullException("hearbeat");
            _hearbeat = hearbeat;
        }

        public void Execute(IJobExecutionContext context)
        {
            _hearbeat.UpdateServiceState("alive");
        }
    }
}
