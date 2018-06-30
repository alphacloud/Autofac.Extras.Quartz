﻿#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2018 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz
{
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using global::Quartz;
    using JetBrains.Annotations;
    using Module = Autofac.Module;

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
    public class QuartzAutofacJobsModule : Module
    {
        [NotNull] readonly Assembly[] _assembliesToScan;


        /// <summary>
        ///     Initializes a new instance of the <see cref="QuartzAutofacJobsModule" /> class.
        /// </summary>
        /// <param name="assembliesToScan">The assemblies to scan for jobs.</param>
        /// <exception cref="System.ArgumentNullException">assembliesToScan</exception>
        public QuartzAutofacJobsModule([NotNull] params Assembly[] assembliesToScan)
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
        [CanBeNull]
        public JobRegistrationFilter JobFilter { get; set; }

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
                .Where(type => !IsAbstract(type) && typeof(IJob).IsAssignableFrom(type) && FilterJob(type))
                .AsSelf().InstancePerLifetimeScope();

            if (AutoWireProperties)
                registrationBuilder.PropertiesAutowired(PropertyWiringOptions);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool FilterJob([NotNull] Type jobType)
        {
            return JobFilter?.Invoke(jobType) ?? true;
        }

        private static bool IsAbstract([NotNull] Type type)
        {
            return type.IsAbstract;
        }
    }
}
