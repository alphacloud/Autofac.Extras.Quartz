Autofac.Extras.Quartz
=====================

Autofac integration package for [Quartz.Net](http://www.quartz-scheduler.net/).


Autofac.Extras.Quartz creates nested litefime scope for each Quartz Job. 
Nested scope is disposed after job execution has been completed.

This allows to have [single instance per job execution](https://github.com/autofac/Autofac/wiki/Instance-Scope#per-lifetime-scope) 
as well as deterministic [disposal of resources](https://github.com/autofac/Autofac/wiki/Deterministic-Disposal).


Install package via Nuget: `install-package Autofac.Extras.Quartz`

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
