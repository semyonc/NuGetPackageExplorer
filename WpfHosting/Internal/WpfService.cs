using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace WpfHosting.Internal
{
    /// <inheritdoc />
    internal class WpfService<T> : IWpfService where T : Application
    {
        private readonly T _application;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;
        private readonly WpfState _state;
        private readonly WpfHostServiceOptions _options;

        public WpfService(ILogger<WpfService<T>> logger, WpfState state, T application, IServiceProvider serviceProvider, IOptions<WpfHostServiceOptions> options)
        {
            _logger = logger;
            _state = state;
            _options = options.Value;

            logger.LogDebug("Constructing WpfService<{type}> with args [{args}]",
                typeof(T).FullName, string.Join(",", state.Arguments));

            _application = application;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public Task<int> RunAsync(CancellationToken cancellationToken)
        {

            try
            {
                var configure = _options.ConfigureApplication;

                if (configure == null)
                {
                    throw new InvalidOperationException($"No application configured. Please specify an application via IHostBuilder.UseStartup.");
                }

                var host = (IHost)_serviceProvider.GetService(typeof(IHost));

                configure(host);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Startup");
                throw;
            }

            _logger.LogDebug("Running");
            // TODO support cancellation tokens. 
            _state.ExitCode = _application.Run();
            return Task.FromResult(_state.ExitCode);
        }
    }
}
