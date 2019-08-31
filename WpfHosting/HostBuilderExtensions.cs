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
    public static class HostExtensions
    {

        public static int RunWithExitCode(this IHost host)
        {
            var state = host.Services.GetRequiredService<WpfState>();
            try
            {
                host.Run();
            }
            catch (OperationCanceledException) { }            

            return state.ExitCode;
        }
    }
}
