using System;
using System.Collections.Generic;
using System.Management.Automation.Host;

namespace PoshCommander
{
    public class PaneView : IPaneView
    {
        private readonly Rectangle bounds;
        private readonly Theme theme = Theme.Default;
        private readonly PSHostUserInterface ui;

        public int FirstVisibleItemIndex { get; set; }
        public int HighlightedIndex { get; set; }
        public IReadOnlyList<FileSystemItem> Items { get; set; } = new List<FileSystemItem>();
        public int MaxVisibleItemCount { get; }
        public PaneState PaneState { get; set; }
        public string StatusText { get; set; }
        public string Title { get; set; }

        public PaneView(Rectangle bounds, PSHostUserInterface ui)
        {
            this.bounds = bounds;
            this.MaxVisibleItemCount = bounds.GetHeight() - 2;
            this.ui = ui;
        }

        public void Redraw()
        {
            DrawTitleBar();
            DrawItems();
            DrawStatusBar();
        }

        public void DrawItems()
        {
            for (var i = 0; i < MaxVisibleItemCount; i++)
            {
                var pos = new Coordinates(bounds.Left, bounds.Top + i + 1);
                var itemIndex = i + FirstVisibleItemIndex;

                var backgroundColor
                    = (PaneState == PaneState.Active && itemIndex == HighlightedIndex)
                        ? theme.RowHightlighedBackground
                    : (itemIndex % 2) == 0
                        ? theme.RowEvenBackground
                    : theme.RowOddBackground;

                var itemStyle = new ConsoleTextStyle(
                    backgroundColor,
                    theme.ItemNormalForeground);

                if (itemIndex < Items.Count)
                {
                    var item = Items[itemIndex];
                    var icon = theme.GetIcon(item);
                    var text = $"{icon} {AnsiEscapeCodes.ForegroundColor(theme.ItemNormalForeground)}{item.Name}";

                    ui.WriteBlockAt(
                        text,
                        pos,
                        bounds.GetWidth(),
                        itemStyle);
                }
                else
                {
                    ui.WriteBlockAt(
                        string.Empty,
                        pos,
                        bounds.GetWidth(),
                        itemStyle);
                }
            }
        }

        public void DrawStatusBar()
        {
            var statusBarStyle = new ConsoleTextStyle(
                theme.StatusBarBackground,
                theme.StatusBarForeground);

            ui.WriteBlockAt(StatusText, bounds.GetBottomLeft(), bounds.GetWidth(), statusBarStyle);
        }

        public void DrawTitleBar()
        {
            ui.WriteBlockAt(
                Title,
                bounds.GetTopLeft(),
                bounds.GetWidth(),
                PaneState == PaneState.Active
                    ? new ConsoleTextStyle(theme.TitleBarActiveBackground, theme.TitleBarActiveForeground)
                    : new ConsoleTextStyle(theme.TitleBarInactiveBackground, theme.TitleBarInactiveForeground));
        }
    }
}
