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
|Build|[![Master branch](https://ci.appveyor.com/api/projects/status/hi40qmgw69rgyot8/branch/master?svg=true)](https://ci.appveyor.com/project/shatl/autofac-extras-quartz/branch/master) | [![Development branch](https://ci.appveyor.com/api/projects/status/hi40qmgw69rgyot8?svg=true)](https://ci.appveyor.com/project/shatl/autofac-extras-quartz)
|NuGet|[![NuGet](https://img.shields.io/nuget/v/Autofac.Extras.Quartz.svg)](Release) | [![NuGet](https://img.shields.io/nuget/vpre/Autofac.Extras.Quartz.svg)](PreRelease)|
|CodeCov|[![codecov](https://codecov.io/gh/alphacloud/Autofac.Extras.Quartz/branch/master/graph/badge.svg)](https://codecov.io/gh/alphacloud/Autofac.Extras.Quartz) | [![codecov](https://codecov.io/gh/alphacloud/Autofac.Extras.Quartz/branch/develop/graph/badge.svg)](https://codecov.io/gh/alphacloud/Autofac.Extras.Quartz/branch/develop) |


# Usage example

Autofac configuration for Quartz consists of two steps:
1. Scheduler registration
2. Job registration

## Scheduler registration

`QuartzAutofacFactoryModule` registers custom `ISchedulerFactory` and default instance of `IScheduler` in Autofac container.

Optionally custom Quartz configuration can be passed using `ConfigurationProvider` property. Provider is callback which returns settings using `NameValueCollection`.

## Job registration
`QuartzAutofacJobsModule` scans given assemblies and registers all non-abstract implementors of `IJob` interface as transient instances.

```
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
```
var scheduler = _container.Resolve<IScheduler>();
scheduler.Start();
```

## Sample projects
* See ``src/Samples/Service`` for windows service implementation based on [TopShelf](https://github.com/Topshelf/Topshelf). **Note:** As of version 0.4.0 TopShelf does not support Quartz.Net v3, so application will throw exception.
* See ``src/Samples/Console`` for .NetCore console application.
* ``src/Samples/Shared`` contains source code shared between samples.

