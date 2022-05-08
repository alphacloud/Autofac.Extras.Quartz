﻿#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2022 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz;

using System.Reflection;

// ReSharper disable once RedundantNameQualifier
/// <summary>
///     Predicate to filter jobs to be registered.
/// </summary>
/// <param name="jobType">Job type class.</param>
/// <returns><c>true</c> if job should be registered, <c>false</c> otherwise.</returns>
public delegate bool JobRegistrationFilter(Type jobType);

/// <summary>
///     Registers Quartz jobs from specified assemblies.
/// </summary>
[PublicAPI]
public class QuartzAutofacJobsModule : global::Autofac.Module
{
    readonly Assembly[] _assembliesToScan;


    /// <summary>
    ///     Initializes a new instance of the <see cref="QuartzAutofacJobsModule" /> class.
    /// </summary>
    /// <param name="assembliesToScan">The assemblies to scan for jobs.</param>
    /// <exception cref="System.ArgumentNullException">assembliesToScan</exception>
    public QuartzAutofacJobsModule(params Assembly[] assembliesToScan)
    {
        _assembliesToScan = assembliesToScan ?? throw new ArgumentNullException(nameof(assembliesToScan));
    }

    /// <summary>
    ///     Instructs Autofac whether registered types should be injected into properties.
    /// </summary>
    /// <remarks>
    ///     Default is <c>false</c>.
    /// </remarks>
    public bool AutoWireProperties { get; set; }

    /// <summary>
    ///     Property wiring options.
    ///     Used if <see cref="AutoWireProperties" /> is <c>true</c>.
    /// </summary>
    /// <remarks>
    ///     See Autofac API documentation http://autofac.org/apidoc/html/33ED0D92.htm for details.
    /// </remarks>
    public PropertyWiringOptions PropertyWiringOptions { get; set; } = PropertyWiringOptions.None;

    /// <summary>
    ///     Job registration filter callback.
    /// </summary>
    /// <seealso cref="JobRegistrationFilter" />
    public JobRegistrationFilter? JobFilter { get; set; }

    /// <summary>
    ///     Override to add registrations to the container.
    /// </summary>
    /// <remarks>
    ///     Note that the ContainerBuilder parameter is unique to this module.
    /// </remarks>
    /// <param name="builder">
    ///     The builder through which components can be
    ///     registered.
    /// </param>
    protected override void Load(ContainerBuilder builder)
    {
        var registrationBuilder = builder.RegisterAssemblyTypes(_assembliesToScan)
            .Where(type => !type.IsAbstract && typeof(IJob).IsAssignableFrom(type) && (JobFilter?.Invoke(type) ?? true))
            .AsSelf().InstancePerLifetimeScope();

        if (AutoWireProperties)
            registrationBuilder.PropertiesAutowired(PropertyWiringOptions);
    }
}
