#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2016 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz
{
    using System;
    using System.Collections.Specialized;
    using global::Quartz;
    using global::Quartz.Core;
    using global::Quartz.Impl;
    using JetBrains.Annotations;

    /// <summary>
    ///     Scheduler factory which uses Autofac to instantiate jobs.
    /// </summary>
    [PublicAPI]
    public class AutofacSchedulerFactory : StdSchedulerFactory
    {
        readonly AutofacJobFactory _jobFactory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Quartz.Impl.StdSchedulerFactory" /> class.
        /// </summary>
        /// <param name="jobFactory">Job factory.</param>
        /// <exception cref="ArgumentNullException"><paramref name="jobFactory" /> is <see langword="null" />.</exception>
        public AutofacSchedulerFactory([NotNull] AutofacJobFactory jobFactory)
        {
            if (jobFactory == null) throw new ArgumentNullException(nameof(jobFactory));
            _jobFactory = jobFactory;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Quartz.Impl.StdSchedulerFactory" /> class.
        /// </summary>
        /// <param name="props">The properties.</param>
        /// <param name="jobFactory">Job factory</param>
        /// <exception cref="ArgumentNullException"><paramref name="jobFactory" /> is <see langword="null" />.</exception>
        public AutofacSchedulerFactory(NameValueCollection props, [NotNull] AutofacJobFactory jobFactory)
            : base(props)
        {
            if (jobFactory == null) throw new ArgumentNullException(nameof(jobFactory));
            _jobFactory = jobFactory;
        }

        /// <summary>
        ///     Instantiates the scheduler.
        /// </summary>
        /// <param name="rsrcs">The resources.</param>
        /// <param name="qs">The scheduler.</param>
        /// <returns>Scheduler.</returns>
        protected override IScheduler Instantiate(QuartzSchedulerResources rsrcs, QuartzScheduler qs)
        {
            var scheduler = base.Instantiate(rsrcs, qs);
            scheduler.JobFactory = _jobFactory;
            return scheduler;
        }
    }
}
