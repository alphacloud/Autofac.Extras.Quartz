#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2015 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz
{
    using System;
    using System.Reflection;
    using global::Quartz;
    using Module = Autofac.Module;

    /// <summary>
    ///     Registers Quartz jobs from specified assemblies.
    /// </summary>
    public class QuartzAutofacJobsModule : Module
    {
        private readonly Assembly[] _assembliesToScan;

        /// <summary>
        ///     Initializes a new instance of the <see cref="QuartzAutofacJobsModule" /> class.
        /// </summary>
        /// <param name="assembliesToScan">The assemblies to scan for jobs.</param>
        /// <exception cref="System.ArgumentNullException">assembliesToScan</exception>
        public QuartzAutofacJobsModule(params Assembly[] assembliesToScan)
        {
            if (assembliesToScan == null) throw new ArgumentNullException("assembliesToScan");
            _assembliesToScan = assembliesToScan;
        }

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
            builder.RegisterAssemblyTypes(_assembliesToScan)
                .Where(type => !type.IsAbstract && typeof (IJob).IsAssignableFrom(type))
                .AsSelf().InstancePerLifetimeScope();
        }
    }
}
