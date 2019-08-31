using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using Microsoft.Extensions.Logging;

namespace WpfHosting.Internal
{
    /// <inheritdoc />
    internal class WpfService<T> : IWpfService where T : Application, IComponentConnector
    {
        private readonly T _application;
        private readonly ILogger _logger;
        private readonly WpfState _state;

        public WpfService(ILogger<WpfService<T>> logger, WpfState state, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _state = state;

            logger.LogDebug("Constructing WpfService<{type}> with args [{args}]",
                typeof(T).FullName, string.Join(",", state.Arguments));

            _application = (T)serviceProvider.GetService(typeof(T));
            _application.InitializeComponent();
        }

        /// <inheritdoc />
        public Task<int> RunAsync(CancellationToken cancellationToken)
        {
            _logger.LogDebug("Running");
            // TODO support cancellation tokens. 
            _state.ExitCode = _application.Run();
            return Task.FromResult(_state.ExitCode);
        }
    }
}
