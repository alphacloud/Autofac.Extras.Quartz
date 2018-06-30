#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2018 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz
{
    using System;
    using System.Collections.Concurrent;
    using System.Globalization;
    using global::Quartz;
    using global::Quartz.Spi;
    using JetBrains.Annotations;

    /// <summary>
    ///     Resolve Quartz Job and it's dependencies from Autofac container.
    /// </summary>
    /// <remarks>
    ///     Factory returns wrapper around read job. It wraps job execution in nested lifetime scope.
    /// </remarks>
    [PublicAPI]
    public class AutofacJobFactory : IJobFactory, IDisposable
    {
        [NotNull] readonly ILifetimeScope _lifetimeScope;

        [NotNull] readonly object _scopeTag;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AutofacJobFactory" /> class.
        /// </summary>
        /// <param name="lifetimeScope">The lifetime scope.</param>
        /// <param name="scopeTag">The tag to use for new scopes.</param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="lifetimeScope" /> or <paramref name="scopeTag" /> is
        ///     <see langword="null" />.
        /// </exception>
        public AutofacJobFactory([NotNull] ILifetimeScope lifetimeScope, [NotNull] object scopeTag)
        {
            _lifetimeScope = lifetimeScope ?? throw new ArgumentNullException(nameof(lifetimeScope));
            _scopeTag = scopeTag ?? throw new ArgumentNullException(nameof(scopeTag));
        }

        internal ConcurrentDictionary<object, JobTrackingInfo> RunningJobs { get; } =
            new ConcurrentDictionary<object, JobTrackingInfo>();

        /// <summary>
        ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            RunningJobs.Clear();
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
        /// <exception cref="ArgumentNullException"><paramref name="bundle" /> is <see langword="null" />.</exception>
        /// <exception cref="ArgumentNullException"><paramref name="scheduler" /> is <see langword="null" />.</exception>
        /// <exception cref="SchedulerConfigException">
        ///     Error resolving exception. Original exception will be stored in
        ///     <see cref="Exception.InnerException" />.
        /// </exception>
        [NotNull]
        public virtual IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            if (bundle == null) throw new ArgumentNullException(nameof(bundle));
            if (scheduler == null) throw new ArgumentNullException(nameof(scheduler));

            var jobType = bundle.JobDetail.JobType;

            var nestedScope = _lifetimeScope.BeginLifetimeScope(_scopeTag);

            IJob newJob;
            try
            {
                newJob = (IJob) nestedScope.Resolve(jobType);
                var jobTrackingInfo = new JobTrackingInfo(nestedScope);
                RunningJobs[newJob] = jobTrackingInfo;
                nestedScope = null;
            }
            catch (Exception ex)
            {
                nestedScope?.Dispose();
                throw new SchedulerConfigException(string.Format(CultureInfo.InvariantCulture,
                    "Failed to instantiate Job '{0}' of type '{1}'",
                    bundle.JobDetail.Key, bundle.JobDetail.JobType), ex);
            }
            return newJob;
        }

        /// <summary>
        ///     Allows the the job factory to destroy/cleanup the job if needed.
        /// </summary>
        public void ReturnJob(IJob job)
        {
            if (job == null)
                return;

            if (!RunningJobs.TryRemove(job, out var trackingInfo))
            {
                // ReSharper disable once SuspiciousTypeConversion.Global
                (job as IDisposable)?.Dispose();
            }
            else
            {
                trackingInfo.Scope?.Dispose();
            }
        }

        #region Job data

        internal sealed class JobTrackingInfo
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
            /// </summary>
            public JobTrackingInfo(ILifetimeScope scope)
            {
                Scope = scope;
            }

            public ILifetimeScope Scope { get; }
        }

        #endregion Job data
    }
}
