using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WpfHosting.Internal
{
    /// <summary>
    ///     A service to be run as part of the <see cref="WpfLifetime" />.
    /// </summary>
    internal interface IWpfService
    {
        /// <summary>
        ///     Runs the application asynchronously and returns the exit code.
        /// </summary>
        /// <param name="cancellationToken">Used to indicate when stop should no longer be graceful.</param>
        /// <returns>The exit code</returns>
        Task<int> RunAsync(CancellationToken cancellationToken);
    }
}
