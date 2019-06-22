using System;

namespace PoshCommander
{
    public class Application
    {
        private readonly IApplicationView view;

        public Pane LeftPane { get; }
        public Pane RightPane { get; }

        public Application(
            string leftPath,
            string rightPath,
            IFileSystem fileSystem,
            ILocationProvider locationProvider,
            IApplicationView view)
        {
            leftPath = leftPath.IsNullOrEmpty()
                ? locationProvider.CurrentLocation
                : locationProvider.ResolvePath(leftPath);

            rightPath = rightPath.IsNullOrEmpty()
                ? locationProvider.CurrentLocation
                : locationProvider.ResolvePath(rightPath);

            this.LeftPane = new Pane(leftPath, fileSystem, PaneState.Active, view.LeftPane);
            this.RightPane = new Pane(rightPath, fileSystem, PaneState.Inactive, view.RightPane);
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
                if (LeftPane.State == PaneState.Active)
                {
                    LeftPane.State = PaneState.Inactive;
                    RightPane.State = PaneState.Active;
                }
                else
                {
                    LeftPane.State = PaneState.Active;
                    RightPane.State = PaneState.Inactive;
                }

            }
            else
            {
                if (LeftPane.State == PaneState.Active)
                    LeftPane.ProcessKey(keyInfo);
                else
                    RightPane.ProcessKey(keyInfo);
            }

            return ApplicationState.Running;
        }
    }
}
