using System;
using System.IO;
using System.Management.Automation;

namespace PoshCommander
{
    [Cmdlet(VerbsLifecycle.Invoke, "PoshCommander")]
    public class PoshCommanderCmdlet : PSCmdlet
    {
        [Parameter(Position = 0)]
        public string LeftPath { get; set; }

        [Parameter(Position = 1)]
        public string RightPath { get; set; }

        protected override void BeginProcessing()
        {
            using (new ConsoleBufferSnapshot(Host.UI.RawUI))
            {
                var applicationView = new ApplicationView(
                    Theme.Default,
                    Host.UI);

                var app = new Application(
                    LeftPath,
                    RightPath,
                    new FileSystem(),
                    new LocationProvider(SessionState.Path),
                    applicationView);

                app.Run();
            }
        }

        private string ResolveDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                throw new ArgumentException($"Directory '{directoryPath}' does not exist");

            return System.IO.Path.GetFullPath(directoryPath);
        }
    }
}
