﻿#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2022 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz;

using System.Collections.Specialized;

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
    public AutofacSchedulerFactory(AutofacJobFactory jobFactory)
    {
        _jobFactory = jobFactory ?? throw new ArgumentNullException(nameof(jobFactory));
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="T:Quartz.Impl.StdSchedulerFactory" /> class.
    /// </summary>
    /// <param name="props">The properties.</param>
    /// <param name="jobFactory">Job factory</param>
    /// <exception cref="ArgumentNullException"><paramref name="jobFactory" /> is <see langword="null" />.</exception>
    public AutofacSchedulerFactory(NameValueCollection props, AutofacJobFactory jobFactory)
        : base(props)
    {
        _jobFactory = jobFactory ?? throw new ArgumentNullException(nameof(jobFactory));
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
