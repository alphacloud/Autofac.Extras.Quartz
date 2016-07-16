#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2016 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz
{
    using System;
    using System.Reflection;
    using Core;
    using global::Quartz;
    using JetBrains.Annotations;
    using Module = Autofac.Module;

    /// <summary>
    ///     Registers Quartz jobs from specified assemblies.
    /// </summary>
    [PublicAPI]
    public class QuartzAutofacJobsModule : Module
    {
        readonly Assembly[] _assembliesToScan;


        /// <summary>
        ///     Initializes a new instance of the <see cref="QuartzAutofacJobsModule" /> class.
        /// </summary>
        /// <param name="assembliesToScan">The assemblies to scan for jobs.</param>
        /// <exception cref="System.ArgumentNullException">assembliesToScan</exception>
        public QuartzAutofacJobsModule([NotNull] params Assembly[] assembliesToScan)
        {
            if (assembliesToScan == null) throw new ArgumentNullException(nameof(assembliesToScan));
            _assembliesToScan = assembliesToScan;
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
                .Where(type => !type.IsAbstract && typeof(IJob).IsAssignableFrom(type))
                .AsSelf().InstancePerLifetimeScope();

            if (AutoWireProperties)
                registrationBuilder.PropertiesAutowired(PropertyWiringOptions);
        }
    }
}
