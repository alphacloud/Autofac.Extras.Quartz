#region copyright
// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014 Alphacloud.Net
#endregion
namespace SimpleService.AppServices
{
    using Common.Logging;

    internal class HeartbeatService: IHeartbeatService
    {
        private static readonly ILog s_log = LogManager.GetLogger<HeartbeatService>();

        public void UpdateServiceState(string state)
        {
            s_log.InfoFormat("Service state: {serviceState}.", state);
        }
    }

    public interface IHeartbeatService
    {
        void UpdateServiceState(string state);
    }
}