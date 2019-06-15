using System;
using System.Management.Automation;
using System.Management.Automation.Host;

namespace PoshCommander
{
    [Cmdlet(VerbsLifecycle.Invoke, "PoshCommander")]
    public class PoshCommanderCmdlet : PSCmdlet
    {
        protected override void BeginProcessing()
        {
            using (new ConsoleBufferSnapshot(Host.UI.RawUI))
            {
                var windowSize = Host.UI.RawUI.WindowSize;
                var leftPaneBounds = new Rectangle(
                    left: 0,
                    top: 0,
                    right: windowSize.Width / 2,
                    bottom: windowSize.Height - 1);

                var rightPaneBounds = new Rectangle(
                    left: leftPaneBounds.Right + 2,
                    top: 0,
                    right: windowSize.Width - 1,
                    bottom: windowSize.Height - 1);

                var leftPane = new Pane(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    leftPaneBounds,
                    Host.UI);

                var rightPane = new Pane(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    rightPaneBounds,
                    Host.UI);

                while (true)
                {
                    var keyInfo = Console.ReadKey(intercept: true);
                    if (keyInfo.Key == ConsoleKey.Q)
                        break;
                }
            }
        }
    }
}
