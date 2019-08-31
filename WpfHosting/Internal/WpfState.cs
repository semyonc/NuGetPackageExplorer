using System;
using System.Collections.Generic;
using System.Text;

namespace WpfHosting.Internal
{
    /// <summary>
    ///     A DI container for storing command line arguments.
    /// </summary>
    internal class WpfState
    {

        public WpfState(string[] args)
        {
            Arguments = args;
        }

        public int ExitCode { get; set; }
        public string[] Arguments { get; }
    }
}
