#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2015 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz
{
    using System;
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

            var jobType = bundle.JobDetail.JobType;
            return jobType.IsAssignableTo<IInterruptableJob>()
                ? new InterruptableJobWrapper(bundle, _lifetimeScope, _scopeName)
                : new JobWrapper(bundle, _lifetimeScope, _scopeName);
        }

        /// <summary>
        ///     Allows the the job factory to destroy/cleanup the job if needed.
        /// </summary>
        public void ReturnJob(IJob job)
        {
        }

        #region Job Wrappers

        internal sealed class InterruptableJobWrapper : JobWrapper, IInterruptableJob
        {
            public InterruptableJobWrapper(TriggerFiredBundle bundle, ILifetimeScope lifetimeScope,
                string scopeName) : base(bundle, lifetimeScope, scopeName)
            {
            }

            public void Interrupt()
            {
                var interruptableJob = RunningJob as IInterruptableJob;
                if (interruptableJob != null)
                    interruptableJob.Interrupt();
            }
        }

        /// <summary>
        ///     Job execution wrapper.
        /// </summary>
        /// <remarks>
        ///     Creates nested lifetime scope per job execution and resolves Job from Autofac.
        /// </remarks>
        internal class JobWrapper : IJob
        {
            private readonly TriggerFiredBundle _bundle;
            private readonly ILifetimeScope _lifetimeScope;
            private readonly string _scopeName;

            /// <summary>
            ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
            /// </summary>
            public JobWrapper(TriggerFiredBundle bundle, ILifetimeScope lifetimeScope,
                string scopeName)
            {
                if (bundle == null) throw new ArgumentNullException("bundle");
                if (lifetimeScope == null) throw new ArgumentNullException("lifetimeScope");
                if (scopeName == null) throw new ArgumentNullException("scopeName");

                _bundle = bundle;
                _lifetimeScope = lifetimeScope;
                _scopeName = scopeName;
            }

            protected IJob RunningJob { get; private set; }

            /// <summary>
            ///     Called by the <see cref="T:Quartz.IScheduler" /> when a <see cref="T:Quartz.ITrigger" />
            ///     fires that is associated with the <see cref="T:Quartz.IJob" />.
            /// </summary>
            /// <remarks>
            ///     The implementation may wish to set a  result object on the
            ///     JobExecutionContext before this method exits.  The result itself
            ///     is meaningless to Quartz, but may be informative to
            ///     <see cref="T:Quartz.IJobListener" />s or
            ///     <see cref="T:Quartz.ITriggerListener" />s that are watching the job's
            ///     execution.
            /// </remarks>
            /// <param name="context">The execution context.</param>
            /// <exception cref="SchedulerConfigException">Job cannot be instantiated.</exception>
            public void Execute(IJobExecutionContext context)
            {
                var scope = _lifetimeScope.BeginLifetimeScope(_scopeName);
                try
                {
                    try
                    {
                        RunningJob = CreateJob(scope);
                    }
                    catch (Exception ex)
                    {
                        throw new SchedulerConfigException(string.Format(CultureInfo.InvariantCulture,
                            "Failed to instantiate Job '{0}' of type '{1}'",
                            _bundle.JobDetail.Key, _bundle.JobDetail.JobType), ex);
                    }

                    RunningJob.Execute(context);
                }
                finally
                {
                    RunningJob = null;
                    scope.Dispose();
                }
            }

            protected virtual IJob CreateJob(ILifetimeScope scope)
            {
                return (IJob) scope.Resolve(_bundle.JobDetail.JobType);
            }
        }

        #endregion
    }
}
