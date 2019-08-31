using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WpfHosting;
using WpfHosting.Internal;

namespace Microsoft.Extensions.Hosting
{

    /// <summary>
    ///     Extension methods for <see cref="IHostBuilder" /> support.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        ///     Runs an instance of <typeparamref name="TApp" />
        /// </summary>
        /// <typeparam name="TApp">The type of the WPF application implementation</typeparam>
        /// <param name="hostBuilder">This instance</param>
        /// <param name="args">The command line arguments</param>
        /// <param name="cancellationToken">A cancellation token</param>
        /// <returns>A task whose result is the exit code of the application</returns>
        public static int RunWpfApplication<TApp>(
            this IHostBuilder hostBuilder, string[] args, CancellationToken cancellationToken = default)
            where TApp : Application, IComponentConnector
        {
            var exceptionHandler = new StoreExceptionHandler();
            var state = new WpfState(args);
            hostBuilder.ConfigureServices(
                (context, services)
                    =>
                {
                    services
                        .TryAddSingleton<IUnhandledExceptionHandler>(exceptionHandler);
                    services
                        .AddSingleton<IHostLifetime, WpfLifetime>();

                    services.AddSingleton(state);
                    services
                        .AddSingleton<TApp>();
                    services
                        .AddSingleton<IWpfService, WpfService<TApp>>();

                });

            using var host = hostBuilder.Build();

            try
            {
                host.RunAsync(cancellationToken).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException) { }            

            return state.ExitCode;
        }
    }
}
