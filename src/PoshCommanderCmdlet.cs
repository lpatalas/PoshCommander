using System.Management.Automation;

namespace PoshCommander
{
    [Cmdlet(VerbsLifecycle.Invoke, "PoshCommander")]
    public class PoshCommanderCmdlet : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            Host.UI.Write("Hello PoshCommander!");
        }
    }
}
