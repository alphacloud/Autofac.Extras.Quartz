Autofac.Extras.Quartz
=====================

Autofac integration for [Quartz.Net](http://www.quartz-scheduler.net/)


Autofac.Extras.Quartz creates nested litefime scope for each Quartz Job. 
Nested scope is disposed after job execution has been completed.

This allows to have [single instance per job execution](https://github.com/autofac/Autofac/wiki/Instance-Scope#per-lifetime-scope) 
as well as deterministic [disposal of resources](https://github.com/autofac/Autofac/wiki/Deterministic-Disposal).


Install package via Nuget: `install-package Autofac.Extras.Quartz`
