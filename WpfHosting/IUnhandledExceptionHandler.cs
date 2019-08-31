using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using WpfHosting.Internal;

namespace WpfHosting
{
    /// <summary>
    /// Used by <see cref="WpfLifetime"/> to handle exceptions that are emitted from the
    /// <see cref="Application"/> e.g. during parsing or execution
    /// </summary>
    public interface IUnhandledExceptionHandler
    {
        /// <summary>
        /// Handle otherwise uncaught exception. You are free to log, rethrow, … the exception 
        /// </summary>
        /// <param name="e">An otherwise uncaught exception</param>
        void HandleException(Exception e);
    }
}
