using System;
using System.Management.Automation.Host;

namespace PoshCommander
{
    public class Application
    {
        private readonly Pane leftPane;
        private readonly Pane rightPane;
        private readonly ApplicationView view;

        public Application(
            string leftPath,
            string rightPath,
            ApplicationView view)
        {
            var fileSystem = new FileSystem();

            this.leftPane = new Pane(leftPath, fileSystem, PaneState.Active, view.LeftPane);
            this.rightPane = new Pane(rightPath, fileSystem, PaneState.Inactive, view.RightPane);
            this.view = view;
        }

        public void Run()
        {
            view.Redraw();

            var applicationState = ApplicationState.Running;
            while (applicationState == ApplicationState.Running)
            {
                var keyInfo = Console.ReadKey(intercept: true);
                applicationState = ProcessKey(keyInfo);
            }
        }

        private ApplicationState ProcessKey(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.Q)
            {
                return ApplicationState.Exiting;
            }
            else if (keyInfo.Key == ConsoleKey.Tab)
            {
                if (leftPane.State == PaneState.Active)
                {
                    leftPane.State = PaneState.Inactive;
                    rightPane.State = PaneState.Active;
                }
                else
                {
                    leftPane.State = PaneState.Active;
                    rightPane.State = PaneState.Inactive;
                }

            }
            else
            {
                if (leftPane.State == PaneState.Active)
                    leftPane.ProcessKey(keyInfo);
                else
                    rightPane.ProcessKey(keyInfo);
            }

            return ApplicationState.Running;
        }
    }
}
