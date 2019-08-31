using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using NuGetPe;

namespace PackageExplorer
{
    internal static class Program
    {
        [STAThread]
        internal static int Main(string[] args)
        {
            DiagnosticsClient.Initialize();

            DiagnosticsClient.TrackEvent("AppStart", new Dictionary<string, string> { { "launchType", args.Length > 0 ? "fileAssociation" : "shortcut" } });


            var retcode = Host.CreateDefaultBuilder()
                .RunWpfApplication<Startup>(args);

            //var app = new Startup();

            //DiagnosticsClient.WireApp(app);

            //app.InitializeComponent();
            //var retcode = app.Run();

            DiagnosticsClient.TrackEvent("AppExit");
            DiagnosticsClient.OnExit();

            return retcode;
        }
    }
}
