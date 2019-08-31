using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using WpfHosting.Internal;

namespace WpfHosting
{
    public class WpfHostBuilder
    {
        private readonly IHostBuilder _builder;
        private readonly IConfiguration _config;
        private readonly object _startupKey = new object();


        public WpfHostBuilder(IHostBuilder builder, string[] args)
        {
            _builder = builder;
            var exceptionHandler = new StoreExceptionHandler();
            var state = new WpfState(args);

            _builder.ConfigureServices((context, services) =>
            {
                services
                    .TryAddSingleton<IUnhandledExceptionHandler>(exceptionHandler);
                services
                    .AddSingleton<IHostLifetime, WpfLifetime>();

                services.AddSingleton(state);
            });
        }

        public IHostBuilder UseStartup<TApp>() where TApp : Application, IComponentConnector
        {
            // UseStartup can be called multiple times. Only run the last one.
            _builder.Properties["UseStartup.StartupType"] = typeof(TApp);
            _builder.ConfigureServices((context, services) =>
            {
                if (_builder.Properties.TryGetValue("UseStartup.StartupType", out var cachedType) && (Type)cachedType == typeof(TApp))
                {
                    services.AddSingleton<IWpfService, WpfService<TApp>>();
                    UseStartup(typeof(TApp), context, services);
                }
            });

            return _builder;
        }

        private void UseStartup(Type startupType, HostBuilderContext context, IServiceCollection services)
        {
            ExceptionDispatchInfo startupError = null;
            object instance = null;
            ConfigureBuilder configureBuilder = null;

            try
            {
                if (StartupLoader.HasConfigureServicesIServiceProviderDelegate(startupType, context.HostingEnvironment.EnvironmentName))
                {
                    throw new NotSupportedException($"ConfigureServices returning an {typeof(IServiceProvider)} isn't supported.");
                }

                instance = ActivatorUtilities.CreateInstance(new HostServiceProvider(context), startupType);
                ((IComponentConnector)instance).InitializeComponent();
                context.Properties[_startupKey] = instance;

                services.AddSingleton(startupType, instance);

                // Startup.ConfigureServices
                var configureServicesBuilder = StartupLoader.FindConfigureServicesDelegate(startupType, context.HostingEnvironment.EnvironmentName);
                var configureServices = configureServicesBuilder.Build(instance);

                configureServices(services);

                // REVIEW: We're doing this in the callback so that we have access to the hosting environment
                // Startup.ConfigureContainer
                var configureContainerBuilder = StartupLoader.FindConfigureContainerDelegate(startupType, context.HostingEnvironment.EnvironmentName);
                if (configureContainerBuilder.MethodInfo != null)
                {
                    var containerType = configureContainerBuilder.GetContainerType();
                    // Store the builder in the property bag
                    _builder.Properties[typeof(ConfigureContainerBuilder)] = configureContainerBuilder;

                    var actionType = typeof(Action<,>).MakeGenericType(typeof(HostBuilderContext), containerType);

                    // Get the private ConfigureContainer method on this type then close over the container type
                    var configureCallback = GetType().GetMethod(nameof(ConfigureContainer), BindingFlags.NonPublic | BindingFlags.Instance)
                                                     .MakeGenericMethod(containerType)
                                                     .CreateDelegate(actionType, this);

                    // _builder.ConfigureContainer<T>(ConfigureContainer);
                    typeof(IHostBuilder).GetMethods().First(m => m.Name == nameof(IHostBuilder.ConfigureContainer))
                        .MakeGenericMethod(containerType)
                        .InvokeWithoutWrappingExceptions(_builder, new object[] { configureCallback });
                }

                // Resolve Configure after calling ConfigureServices and ConfigureContainer
                configureBuilder = StartupLoader.FindConfigureDelegate(startupType, context.HostingEnvironment.EnvironmentName);
            }
            catch (Exception ex)
            {
                startupError = ExceptionDispatchInfo.Capture(ex);
            }

            // Startup.Configure
            services.Configure<WpfHostServiceOptions>(options =>
            {
                options.ConfigureApplication = app =>
                {
                    // Throw if there was any errors initializing startup
                    startupError?.Throw();

                    // Execute Startup.Configure
                    if (instance != null && configureBuilder != null)
                    {
                        configureBuilder.Build(instance)(app);
                    }
                };
            });
        }


        private void ConfigureContainer<TContainer>(HostBuilderContext context, TContainer container)
        {
            var instance = context.Properties[_startupKey];
            var builder = (ConfigureContainerBuilder)context.Properties[typeof(ConfigureContainerBuilder)];
            builder.Build(instance)(container);
        }

        // This exists just so that we can use ActivatorUtilities.CreateInstance on the Startup class
        private class HostServiceProvider : IServiceProvider
        {
            private readonly HostBuilderContext _context;

            public HostServiceProvider(HostBuilderContext context)
            {
                _context = context;
            }

            public object GetService(Type serviceType)
            {
                // The implementation of the HostingEnvironment supports both interfaces

#pragma warning disable CS0618 // Type or member is obsolete
                if (serviceType == typeof(IHostingEnvironment)
#pragma warning restore CS0618 // Type or member is obsolete
                    || serviceType == typeof(IHostEnvironment)
                    )
                {
                    return _context.HostingEnvironment;
                }

                if (serviceType == typeof(IConfiguration))
                {
                    return _context.Configuration;
                }

                return null;
            }
        }
    }
}
