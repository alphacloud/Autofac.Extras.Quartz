#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2022 Alphacloud.Net

#endregion

namespace Autofac.Extras.Quartz;

using System.Collections.Specialized;

/// <summary>
///     Provides additional configuration to Quartz scheduler.
/// </summary>
/// <param name="componentContext"></param>
/// <returns>Quartz configuration settings.</returns>
public delegate NameValueCollection QuartzConfigurationProvider(IComponentContext componentContext);

/// <summary>
///     Configures scheduler job scope.
/// </summary>
/// <remarks>
///     Used to override global container registrations at job scope.
/// </remarks>
/// <param name="containerBuilder">Autofac container builder.</param>
/// <param name="scopeTag">Job scope tag.</param>
public delegate void QuartzJobScopeConfigurator(ContainerBuilder containerBuilder, object scopeTag);

/// <summary>
///     Registers <see cref="ISchedulerFactory" /> and default <see cref="IScheduler" />.
/// </summary>
[PublicAPI]
public class QuartzAutofacFactoryModule : Module
{
    /// <summary>
    ///     Default name for nested lifetime scope.
    /// </summary>
    public static readonly string LifetimeScopeName = "quartz.job";

    readonly string _lifetimeScopeTag;

    /// <summary>
    ///     Initializes a new instance of the <see cref="QuartzAutofacFactoryModule" /> class with a default lifetime scope
    ///     name.
    /// </summary>
    /// <exception cref="System.ArgumentNullException">lifetimeScopeName</exception>
    public QuartzAutofacFactoryModule()
        : this(LifetimeScopeName)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="QuartzAutofacFactoryModule" /> class.
    /// </summary>
    /// <param name="lifetimeScopeTag">Tag of the lifetime scope to wrap job resolution and execution.</param>
    /// <exception cref="System.ArgumentNullException">lifetimeScopeName</exception>
    public QuartzAutofacFactoryModule(string lifetimeScopeTag)
    {
        _lifetimeScopeTag = lifetimeScopeTag ?? throw new ArgumentNullException(nameof(lifetimeScopeTag));
    }

    /// <summary>
    ///     Provides custom configuration for Scheduler.
    ///     Returns <see cref="NameValueCollection" /> with custom Quartz settings.
    ///     <para>See http://quartz-scheduler.org/documentation/quartz-2.x/configuration/ for settings description.</para>
    ///     <seealso cref="StdSchedulerFactory" /> for some configuration property names.
    /// </summary>
    public QuartzConfigurationProvider? ConfigurationProvider { get; set; }

    /// <summary>
    ///     Allows to override job scope registrations.
    /// </summary>
    public QuartzJobScopeConfigurator? JobScopeConfigurator { get; set; }

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
        builder.Register(c =>
                new AutofacJobFactory(c.Resolve<ILifetimeScope>(), _lifetimeScopeTag, JobScopeConfigurator))
            .AsSelf()
            .As<IJobFactory>()
            .SingleInstance();

        builder.Register<ISchedulerFactory>(c => {
                var cfgProvider = ConfigurationProvider;

                var autofacSchedulerFactory = cfgProvider != null
                    ? new AutofacSchedulerFactory(cfgProvider(c), c.Resolve<AutofacJobFactory>())
                    : new AutofacSchedulerFactory(c.Resolve<AutofacJobFactory>());
                return autofacSchedulerFactory;
            })
            .SingleInstance();

        builder.Register(c => {
                var factory = c.Resolve<ISchedulerFactory>();
                return factory.GetScheduler().ConfigureAwait(false).GetAwaiter().GetResult();
            })
            .As<IScheduler>()
            .SingleInstance();
    }
}
