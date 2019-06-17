using System;
using System.Collections.Generic;
using System.Management.Automation.Host;
using System.Text;

namespace PoshCommander
{
    public class ApplicationView
    {
        private readonly Theme theme;
        private readonly PSHostUserInterface ui;

        public int SeparatorPosition { get; set; }
        public Size WindowSize { get; set; }

        public ApplicationView(
            int separatorPosition,
            Theme theme,
            PSHostUserInterface ui,
            Size windowSize)
        {
            this.SeparatorPosition = separatorPosition;
            this.theme = theme;
            this.ui = ui;
            this.WindowSize = windowSize;
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
