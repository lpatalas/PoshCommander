using System.Management.Automation.Host;

namespace PoshCommander.UI
{
    public class ApplicationView : IApplicationView
    {
        private readonly Theme theme;
        private readonly PSHostUserInterface ui;

        public IPaneView LeftPane { get; }
        public IPaneView RightPane { get; }
        public int SeparatorPosition { get; set; }
        public Size WindowSize { get; set; }

        public ApplicationView(
            Theme theme,
            PSHostUserInterface ui)
        {
            this.SeparatorPosition = ui.RawUI.WindowSize.Width / 2;
            this.theme = theme;
            this.ui = ui;
            this.WindowSize = ui.RawUI.WindowSize;

            var leftPaneBounds = new Rectangle(
                    left: 0,
                    top: 0,
                    right: WindowSize.Width / 2 - 1,
                    bottom: WindowSize.Height - 1);

            var rightPaneBounds = new Rectangle(
                left: leftPaneBounds.Right + 2,
                top: 0,
                right: WindowSize.Width - 2,
                bottom: WindowSize.Height - 1);

            LeftPane = new PaneView(leftPaneBounds, theme, ui);
            RightPane = new PaneView(rightPaneBounds, theme, ui);
        }

        public void Redraw()
        {
            for (var y = 0; y < WindowSize.Height - 1; y++)
            {
                ui.WriteAt(
                    "\u2502",
                    new Coordinates(SeparatorPosition, y),
                    new ConsoleTextStyle(theme.SeparatorBackground, theme.SeparatorForeground));
            }

            ui.WriteAt(
                "\u2502",
                new Coordinates(SeparatorPosition, WindowSize.Height - 1),
                new ConsoleTextStyle(theme.StatusBarBackground, theme.SeparatorForeground));
        }
    }
}
