#region copyright

// Autofac Quartz integration
// https://github.com/alphacloud/Autofac.Extras.Quartz
// Licensed under MIT license.
// Copyright (c) 2014-2022 Alphacloud.Net

#endregion

// ReSharper disable once CheckNamespace
namespace SimpleService.AppServices;

[PublicAPI]
public interface IScopedDependency
{
    string Scope { get; }
}

class ScopedDependency : IScopedDependency
{
    public ScopedDependency(string scope)
    {
        Scope = scope ?? throw new ArgumentNullException(nameof(scope));
    }

    public string Scope { get; }
}
