﻿#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2015 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using global::Quartz;
    using global::Quartz.Spi;

    /// <summary>
    ///     Resolve Quartz Job and it's dependencies from Autofac container.
    /// </summary>
    /// <remarks>
    ///     Factory retuns wrapper around read job. It wraps job execution in nested lifetime scope.
    /// </remarks>
    public class AutofacJobFactory : IJobFactory
    {
        private readonly ILifetimeScope _lifetimeScope;
        private readonly string _scopeName;

        readonly ConcurrentDictionary<IJob, ILifetimeScope> scopes = new ConcurrentDictionary<IJob, ILifetimeScope> ();

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutofacJobFactory" /> class.
        /// </summary>
        /// <param name="lifetimeScope">The lifetime scope.</param>
        /// <param name="scopeName">Name of the scope.</param>
        public AutofacJobFactory(ILifetimeScope lifetimeScope, string scopeName)
        {
            if (lifetimeScope == null) throw new ArgumentNullException("lifetimeScope");
            if (scopeName == null) throw new ArgumentNullException("scopeName");
            _lifetimeScope = lifetimeScope;
            _scopeName = scopeName;
        }

        /// <summary>
        ///     Called by the scheduler at the time of the trigger firing, in order to
        ///     produce a <see cref="T:Quartz.IJob" /> instance on which to call Execute.
        /// </summary>
        /// <remarks>
        ///     It should be extremely rare for this method to throw an exception -
        ///     basically only the the case where there is no way at all to instantiate
        ///     and prepare the Job for execution.  When the exception is thrown, the
        ///     Scheduler will move all triggers associated with the Job into the
        ///     <see cref="F:Quartz.TriggerState.Error" /> state, which will require human
        ///     intervention (e.g. an application restart after fixing whatever
        ///     configuration problem led to the issue wih instantiating the Job.
        /// </remarks>
        /// <param name="bundle">
        ///     The TriggerFiredBundle from which the <see cref="T:Quartz.IJobDetail" />
        ///     and other info relating to the trigger firing can be obtained.
        /// </param>
        /// <param name="scheduler">a handle to the scheduler that is about to execute the job</param>
        /// <throws>SchedulerException if there is a problem instantiating the Job. </throws>
        /// <returns>
        ///     the newly instantiated Job
        /// </returns>
        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            if (bundle == null) throw new ArgumentNullException("bundle");
            if (scheduler == null) throw new ArgumentNullException("scheduler");

            try {
                var scope = _lifetimeScope.BeginLifetimeScope (_scopeName);

                var job = (IJob) scope.Resolve (bundle.JobDetail.JobType);
                scopes [job] = scope;

                return job;
            } catch (Exception ex) {
                throw new SchedulerConfigException (string.Format (CultureInfo.InvariantCulture, "Problem instantiating class '{0}'", bundle.JobDetail.JobType.FullName), ex);
            }
        }


        /// <summary>
        ///     Allows the the job factory to destroy/cleanup the job if needed.
        /// </summary>
        public void ReturnJob (IJob job)
        {
            ILifetimeScope scope;
            if (scopes.TryRemove (job, out scope)) {
                scope.Dispose ();
            }
        }
    }
}
