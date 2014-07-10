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
    using global::Quartz.Spi;
    using JetBrains.Annotations;


    /// <summary>
    ///     Registers <see cref="ISchedulerFactory" /> and default <see cref="IScheduler" />.
    /// </summary>
    [PublicAPI]
    public class QuartzAutofacFactoryModule : Module
    {
        /// <summary>
        ///     Default name for nested lifetime scope.
        /// </summary>
        [PublicAPI] public const string LifetimeScopeName = "quartz.job";

        private readonly string _lifetimeScopeName;


        /// <summary>
        ///     Initializes a new instance of the <see cref="QuartzAutofacFactoryModule" /> class with a default lifetime scope
        ///     name.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">lifetimeScopeName</exception>
        public QuartzAutofacFactoryModule()
            : this(LifetimeScopeName)
        {}


        /// <summary>
        ///     Initializes a new instance of the <see cref="QuartzAutofacFactoryModule" /> class.
        /// </summary>
        /// <param name="lifetimeScopeName">Name of the lifetime scope to wrap job resolution and execution.</param>
        /// <exception cref="System.ArgumentNullException">lifetimeScopeName</exception>
        public QuartzAutofacFactoryModule([NotNull] string lifetimeScopeName)
        {
            if (lifetimeScopeName == null) throw new ArgumentNullException("lifetimeScopeName");
            _lifetimeScopeName = lifetimeScopeName;
        }


        /// <summary>
        ///    Provides custom configuration for Scheduler.
        /// </summary>
        [PublicAPI]
        public Func<NameValueCollection> ConfigurationProvider { get; set; }


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
            builder.Register(c => new AutofacJobFactory(c.Resolve<ILifetimeScope>(), _lifetimeScopeName))
                .AsSelf()
                .As<IJobFactory>()
                .SingleInstance();

            builder.Register<ISchedulerFactory>(c => {
                var cfgProvider = ConfigurationProvider;

                var autofacSchedulerFactory = (cfgProvider != null)
                    ? new AutofacSchedulerFactory(cfgProvider(), c.Resolve<AutofacJobFactory>())
                    : new AutofacSchedulerFactory(c.Resolve<AutofacJobFactory>());
                return autofacSchedulerFactory;
            })
                .SingleInstance();

            builder.Register(c => c.Resolve<ISchedulerFactory>().GetScheduler())
                .SingleInstance();
        }
    }
}
