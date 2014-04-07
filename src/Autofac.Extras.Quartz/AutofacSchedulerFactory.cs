#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014 Alphacloud.Net

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
        private readonly ILifetimeScope _lifetimeScope;
        private readonly string _scopeName;


        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Quartz.Impl.StdSchedulerFactory" /> class.
        /// </summary>
        public AutofacSchedulerFactory([NotNull] ILifetimeScope lifetimeScope, [NotNull] string scopeName = "quartz.job")
        {
            if (lifetimeScope == null) throw new ArgumentNullException("lifetimeScope");
            if (scopeName == null) throw new ArgumentNullException("scopeName");
            _lifetimeScope = lifetimeScope;
            _scopeName = scopeName;
        }


        /// <summary>
        ///     Initializes a new instance of the <see cref="T:Quartz.Impl.StdSchedulerFactory" /> class.
        /// </summary>
        /// <param name="props">The properties.</param>
        /// <param name="lifetimeScope"></param>
        /// <param name="scopeName"></param>
        public AutofacSchedulerFactory(NameValueCollection props, [NotNull] ILifetimeScope lifetimeScope,
            [NotNull] string scopeName = "quartz.job")
            : base(props)
        {
            if (lifetimeScope == null) throw new ArgumentNullException("lifetimeScope");
            if (scopeName == null) throw new ArgumentNullException("scopeName");
            _lifetimeScope = lifetimeScope;
            _scopeName = scopeName;
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
            scheduler.JobFactory = new AutofacJobFactory(_lifetimeScope, _scopeName);
            return scheduler;
        }
    }
}