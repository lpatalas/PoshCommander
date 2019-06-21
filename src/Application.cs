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
            PSHostUserInterface ui)
        {
            var fileSystem = new FileSystem();
            var windowSize = ui.RawUI.WindowSize;

            var leftPaneBounds = new Rectangle(
                    left: 0,
                    top: 0,
                    right: windowSize.Width / 2 - 1,
                    bottom: windowSize.Height - 1);

            var leftPaneView = new PaneView(leftPaneBounds, ui);
            leftPane = new Pane(leftPath, fileSystem, PaneState.Active, leftPaneView);

            var rightPaneBounds = new Rectangle(
                left: leftPaneBounds.Right + 2,
                top: 0,
                right: windowSize.Width - 2,
                bottom: windowSize.Height - 1);

            var rightPaneView = new PaneView(rightPaneBounds, ui);
            rightPane = new Pane(rightPath, fileSystem, PaneState.Inactive, rightPaneView);

            view = new ApplicationView(
                windowSize.Width / 2,
                Theme.Default,
                ui,
                windowSize);
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
