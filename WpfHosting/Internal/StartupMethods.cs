using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WpfHosting.Internal
{
    internal class StartupMethods
    {
        public StartupMethods(object instance, Action<IHost> configure, Func<IServiceCollection, IServiceProvider> configureServices)
        {
            Debug.Assert(configure != null);
            Debug.Assert(configureServices != null);

            StartupInstance = instance;
            ConfigureDelegate = configure;
            ConfigureServicesDelegate = configureServices;
        }

        public object StartupInstance { get; }
        public Func<IServiceCollection, IServiceProvider> ConfigureServicesDelegate { get; }
        public Action<IHost> ConfigureDelegate { get; }

    }
}
