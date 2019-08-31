using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Hosting;

namespace WpfHosting.Internal
{
    /// <summary>
    ///     Waits from completion of the <see cref="Application" /> and
    ///     initiates shutdown.
    /// </summary>
    internal class WpfLifetime : IHostLifetime, IDisposable
    {
        private readonly IHostApplicationLifetime _applicationLifetime;
        private readonly IWpfService _wpfService;
        private readonly IUnhandledExceptionHandler? _unhandledExceptionHandler;
        private readonly ManualResetEvent _blockProcessExit = new ManualResetEvent(false);

        /// <summary>
        ///     Creates a new instance.
        /// </summary>
        public WpfLifetime(IHostApplicationLifetime applicationLifetime,
                           IWpfService wpfService,
                           IUnhandledExceptionHandler? unhandledExceptionHandler = null)
        {
            _applicationLifetime = applicationLifetime;
            _wpfService = wpfService;
            _unhandledExceptionHandler = unhandledExceptionHandler;
        }

        /// <summary>The exit code returned by the command line application</summary>
        public int ExitCode { get; private set; }

        /// <inheritdoc />
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        ///     Registers an <code>ApplicationStarted</code> hook that runs the
        ///     <see cref="IWpfService" />. This ensures the container and all
        ///     hosted services are started before the
        ///     <see cref="Application" /> is run.  After the
        ///     <code>ICliService</code> completes, the <code>ExitCode</code> is
        ///     recorded and the application is stopped.
        /// </summary>
        /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
        /// <returns></returns>
        /// <seealso cref="IHostLifetime.WaitForStartAsync(CancellationToken)" />
        public Task WaitForStartAsync(CancellationToken cancellationToken)
        {
            _applicationLifetime.ApplicationStarted.Register(async () =>
            {
                try
                {
                    ExitCode = await _wpfService.RunAsync(cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    if (_unhandledExceptionHandler != null)
                    {
                        _unhandledExceptionHandler.HandleException(e);
                    }
                    else
                    {
                        ExceptionDispatchInfo.Capture(e).Throw();
                    }
                }
                finally
                {
                    _applicationLifetime.StopApplication();
                }
            });


            AppDomain.CurrentDomain.ProcessExit += (_, __) =>
            {
                _applicationLifetime.StopApplication();
                // Ensures services are disposed before the application exits.
                _blockProcessExit.WaitOne();
            };

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _blockProcessExit.Set();
        }
    }
}
