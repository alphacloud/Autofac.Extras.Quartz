Autofac.Extras.Quartz
=====================

Autofac integration package for [Quartz.Net](http://www.quartz-scheduler.net/).

Autofac.Extras.Quartz creates nested litefime scope for each Quartz Job. 
Nested scope is disposed after job execution has been completed.

This allows to have [single instance per job execution](https://github.com/autofac/Autofac/wiki/Instance-Scope#per-lifetime-scope) 
as well as deterministic [disposal of resources](https://github.com/autofac/Autofac/wiki/Deterministic-Disposal).

Install package via Nuget: `install-package Autofac.Extras.Quartz`

## Build status

||Stable|Pre-release|
|:--:|:--:|:--:|
|Build|[![Master branch](https://ci.appveyor.com/api/projects/status/hi40qmgw69rgyot8/branch/master?svg=true)](https://ci.appveyor.com/project/shatl/autofac-extras-quartz/branch/master) | [![Development branch](https://ci.appveyor.com/api/projects/status/hi40qmgw69rgyot8?svg=true)](https://ci.appveyor.com/project/shatl/autofac-extras-quartz) |
|NuGet|[![NuGet](https://img.shields.io/nuget/v/Autofac.Extras.Quartz.svg)](https://www.nuget.org/packages/Autofac.Extras.Quartz) | [![NuGet](https://img.shields.io/nuget/vpre/Autofac.Extras.Quartz.svg)](https://www.nuget.org/packages/Autofac.Extras.Quartz/absoluteLatest) |
|CodeCov|[![codecov](https://codecov.io/gh/alphacloud/Autofac.Extras.Quartz/branch/master/graph/badge.svg)](https://codecov.io/gh/alphacloud/Autofac.Extras.Quartz) | [![codecov](https://codecov.io/gh/alphacloud/Autofac.Extras.Quartz/branch/develop/graph/badge.svg)](https://codecov.io/gh/alphacloud/Autofac.Extras.Quartz/branch/develop) |


# Usage example

Autofac configuration for Quartz includes two steps:
1. Scheduler registration
2. Job registration

## Scheduler registration

`QuartzAutofacFactoryModule` registers custom `ISchedulerFactory` and default instance of `IScheduler` in Autofac container.
Both factory and schedulere are registered as singletones.
*Note:* Is is important to resolve `IScheduler` from container, rather than using default one to get jobs resolved by Autofac.

Optionally custom Quartz configuration can be passed using `ConfigurationProvider` property. Provider is callback which returns settings using `NameValueCollection`.

### Job scope configuration

Starting with version 7 `QuartzAutofacFactoryModule` provides a way to customize lifetime scope configuration for job. This can be done via `JobScopeConfigurator` parameter.

```csharp
cb.Register(_ => new ScopedDependency("global"))
    .AsImplementedInterfaces()
    .SingleInstance();

cb.RegisterModule(new QuartzAutofacFactoryModule {
    JobScopeConfigurator = (builder, jobScopeTag) => {
        // override dependency for job scope
        builder.Register(_ => new ScopedDependency("job-local "+ DateTime.UtcNow.ToLongTimeString()))
            .AsImplementedInterfaces()
            .InstancePerMatchingLifetimeScope(jobScopeTag);

    }
});
```
## Scheduler registration

`QuartzAutofacFactoryModule` registers custom `ISchedulerFactory` and default instance of `IScheduler` in Autofac container.
Both factory and schedulere are registered as singletones.

*Note:* Is is important to resolve `IScheduler` from container, rather than using default one to get jobs resolved by Autofac.

Optionally custom Quartz configuration can be passed using `ConfigurationProvider` property. Provider is callback which returns settings using `NameValueCollection`.

### Job scope configuration
Starting with version 7 `QuartzAutofacFactoryModule` provides a way to customize lifetime scope configuration for job. This can be done via `JobScopeConfigurator` parameter.

```csharp
            cb.Register(_ => new ScopedDependency("global"))
                .AsImplementedInterfaces()
                .SingleInstance();

            cb.RegisterModule(new QuartzAutofacFactoryModule {
                JobScopeConfigurator = (builder, tag) => {
                    // override dependency for job scope
                    builder.Register(_ => new ScopedDependency("job-local "+ DateTime.UtcNow.ToLongTimeString()))
                        .AsImplementedInterfaces()
                        .InstancePerMatchingLifetimeScope(tag);

                }
            });


```
## Scheduler registration

`QuartzAutofacFactoryModule` registers custom `ISchedulerFactory` and default instance of `IScheduler` in Autofac container.
Both factory and schedulere are registered as singletones.
*Note:* Is is important to resolve `IScheduler` from container, rather than using default one to get jobs resolved by Autofac.

Optionally custom Quartz configuration can be passed using `ConfigurationProvider` property. Provider is callback which returns settings using `NameValueCollection`.

### Job scope configuration
Starting with version 7 `QuartzAutofacFactoryModule` provides a way to customize lifetime scope configuration for job. This can be done via `JobScopeConfigurator` parameter.

```csharp
            cb.Register(_ => new ScopedDependency("global"))
                .AsImplementedInterfaces()
                .SingleInstance();

            cb.RegisterModule(new QuartzAutofacFactoryModule {
                JobScopeConfigurator = (builder, tag) => {
                    // override dependency for job scope
                    builder.Register(_ => new ScopedDependency("job-local "+ DateTime.UtcNow.ToLongTimeString()))
                        .AsImplementedInterfaces()
                        .InstancePerMatchingLifetimeScope(tag);

                }
            });


```
## Scheduler registration

`QuartzAutofacFactoryModule` registers custom `ISchedulerFactory` and default instance of `IScheduler` in Autofac container.
Both factory and schedulere are registered as singletones.

*Note:* Is is important to resolve `IScheduler` from container, rather than using default one to get jobs resolved by Autofac.

Optionally custom Quartz configuration can be passed using `ConfigurationProvider` property. Provider is callback which returns settings using `NameValueCollection`.

### Job scope configuration
Starting with version 7 `QuartzAutofacFactoryModule` provides a way to customize lifetime scope configuration for job. This can be done via `JobScopeConfigurator` parameter.

```csharp
cb.Register(_ => new ScopedDependency("global"))
    .AsImplementedInterfaces()
    .SingleInstance();

cb.RegisterModule(new QuartzAutofacFactoryModule {
    JobScopeConfigurator = (builder, tag) => {
        // override dependency for job scope
        builder.Register(_ => new ScopedDependency("job-local "+ DateTime.UtcNow.ToLongTimeString()))
            .AsImplementedInterfaces()
            .InstancePerMatchingLifetimeScope(tag);
    }
});
```
See [src/Samples/Shared/Bootstrap.cs](src/Samples/Shared/Bootstrap.cs) for details.


## Job registration
`QuartzAutofacJobsModule` scans given assemblies and registers all non-abstract implementors of `IJob` interface as transient instances.

```csharp
internal static ContainerBuilder ConfigureContainer(ContainerBuilder cb)
{
	// 1) Register IScheduler
	cb.RegisterModule(new QuartzAutofacFactoryModule()); 
	// 2) Register jobs
	cb.RegisterModule(new QuartzAutofacJobsModule(typeof (CleanupExpiredAnnouncemetsJob).Assembly));
}
```

## Starting Quartz
Make sure to start the scheduler after it was resolved from Autofac. E.g.
```csharp
var scheduler = _container.Resolve<IScheduler>();
scheduler.Start();
```

## Sample projects
* See [src/Samples/Console](src/Samples/Console/) for .NetCore console application.
* [src/Samples/Shared](src/Samples/Shared/) contains source code shared between samples.

TopShelf-based sample was removed since Topshelf.Quartz is not compatible with Quartz 3 as af now.