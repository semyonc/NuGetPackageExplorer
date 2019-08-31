using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NuGetPe;
using WpfHosting;

namespace PackageExplorer
{
    internal static class Program
    {
        [STAThread]
        internal static int Main(string[] args)
        {
            DiagnosticsClient.Initialize();

            DiagnosticsClient.TrackEvent("AppStart", new Dictionary<string, string> { { "launchType", args.Length > 0 ? "fileAssociation" : "shortcut" } });


            var host = new WpfHostBuilder(Host.CreateDefaultBuilder(), args)
                .UseStartup<Startup>()
                .Build();

            var retcode = host.RunWithExitCode();


            DiagnosticsClient.TrackEvent("AppExit");
            DiagnosticsClient.OnExit();

            return retcode;
        }
    }
}
