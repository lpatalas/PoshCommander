using System.Management.Automation;

namespace PoshCommander
{
    [Cmdlet(VerbsLifecycle.Invoke, "PoshCommander")]
    public class PoshCommanderCmdlet : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            using (new ConsoleBufferSnapshot(Host.UI.RawUI))
            {
                var app = new Application(Host.UI);
                app.Run();
            }
        }
    }
}
