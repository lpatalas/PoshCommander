using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;

namespace PoshCommander
{
    public class Pane
    {
        private static readonly ConsoleTextStyle itemStyleEven
            = new ConsoleTextStyle(new RgbColor(29, 40, 61), RgbColor.White);
        private static readonly ConsoleTextStyle itemStyleOdd
            = new ConsoleTextStyle(new RgbColor(33, 45, 67), RgbColor.White);
        private static readonly ConsoleTextStyle itemStyleHighlighted
            = new ConsoleTextStyle(new RgbColor(26, 64, 105), RgbColor.White);
        private static readonly ConsoleTextStyle statusBarStyle
            = new ConsoleTextStyle(new RgbColor(80, 80, 80), new RgbColor(200, 200, 200));
        private static readonly ConsoleTextStyle titleBarStyleActive
            = new ConsoleTextStyle(new RgbColor(64, 64, 192), new RgbColor(255, 255, 128));
        private static readonly ConsoleTextStyle titleBarStyleInactive
            = new ConsoleTextStyle(new RgbColor(64, 64, 64), new RgbColor(128, 128, 128));

        private readonly Rectangle bounds;
        private string currentDirectoryPath;
        private int firstVisibleItemIndex;
        private int highlightedIndex;
        private IReadOnlyList<FileSystemItem> items;
        private readonly int maxVisibleItemCount;
        private readonly PSHostUserInterface ui;

        public PaneState State { get; private set; }

        public Pane(
            string directoryPath,
            Rectangle bounds,
            PaneState paneState,
            PSHostUserInterface ui)
        {
            this.bounds = bounds;
            this.State = paneState;
            this.maxVisibleItemCount = bounds.GetHeight() - 2;
            this.ui = ui;

            ChangeDirectory(directoryPath, redraw: false);
        }

        public void Activate()
        {
            State = PaneState.Active;
            DrawTitleBar();
        }

        public void Deactivate()
        {
            State = PaneState.Inactive;
            DrawTitleBar();
        }

        public void ProcessKey(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.UpArrow)
                highlightedIndex = Math.Max(0, highlightedIndex - 1);
            else if (keyInfo.Key == ConsoleKey.DownArrow)
                highlightedIndex = Math.Min(items.Count - 1, highlightedIndex + 1);
            else if (keyInfo.Key == ConsoleKey.PageUp)
                highlightedIndex = Math.Max(0, highlightedIndex - maxVisibleItemCount + 1);
            else if (keyInfo.Key == ConsoleKey.PageDown)
                highlightedIndex = Math.Min(items.Count - 1, highlightedIndex + maxVisibleItemCount - 1);
            else if (keyInfo.Key == ConsoleKey.Home)
                highlightedIndex = 0;
            else if (keyInfo.Key == ConsoleKey.End)
                highlightedIndex = items.Count - 1;
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                var highlightedItem = items[highlightedIndex];
                if (highlightedItem.Kind == FileSystemItemKind.Directory)
                {
                    ChangeDirectory(highlightedItem.FullPath, true);
                    return;
                }
            }

            ScrollToHighlightedItem();
            DrawItems();
        }

        private void ScrollToHighlightedItem()
        {
            if (highlightedIndex < firstVisibleItemIndex)
                firstVisibleItemIndex = highlightedIndex;
            else if (highlightedIndex >= firstVisibleItemIndex + maxVisibleItemCount)
                firstVisibleItemIndex = highlightedIndex - maxVisibleItemCount + 1;
        }

        public void Redraw()
        {
            DrawTitleBar();
            DrawItems();
            DrawStatusBar();
        }

        private void ChangeDirectory(string directoryPath, bool redraw)
        {
            var previousDirectoryPath = currentDirectoryPath;
            currentDirectoryPath = directoryPath;
            firstVisibleItemIndex = 0;
            items = CreateItemList(directoryPath);

            highlightedIndex = items
                .FirstIndexOf(item => string.Equals(item.FullPath, previousDirectoryPath, StringComparison.OrdinalIgnoreCase))
                .GetValueOrDefault(0);

            ScrollToHighlightedItem();

            if (redraw)
                Redraw();
        }

        private void DrawItems()
        {
            for (var i = 0; i < maxVisibleItemCount; i++)
            {
                var pos = new Coordinates(bounds.Left, bounds.Top + i + 1);
                var itemIndex = i + firstVisibleItemIndex;
                var itemStyle
                    = itemIndex == highlightedIndex ? itemStyleHighlighted
                    : (itemIndex % 2) == 0 ? itemStyleEven
                    : itemStyleOdd;

                if (itemIndex < items.Count)
                {
                    var item = items[itemIndex];
                    var icon
                        = item.Kind == FileSystemItemKind.Directory ? "\uF74A"
                        : item.Kind == FileSystemItemKind.File ? "\uF723"
                        : item.Kind == FileSystemItemKind.SymbolicLink ? "\uF751"
                        : throw new InvalidOperationException($"Invalid enum value: {item.Kind}");

                    var text = $"{icon} {item.Name}";

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

        private void DrawStatusBar()
        {
            var fileCount = items.Count(item => item.Kind == FileSystemItemKind.File);
            var directoryCount = items.Count(item => item.Kind == FileSystemItemKind.Directory);
            var text = $"Files: {fileCount}, Directories: {directoryCount}";
            ui.WriteBlockAt(text, bounds.GetBottomLeft(), bounds.GetWidth(), statusBarStyle);
        }

        private void DrawTitleBar()
        {
            ui.WriteBlockAt(
                currentDirectoryPath,
                bounds.GetTopLeft(),
                bounds.GetWidth(),
                State == PaneState.Active
                    ? titleBarStyleActive
                    : titleBarStyleInactive);
        }

        private static IReadOnlyList<FileSystemItem> CreateItemList(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            var parentItems
                = directoryInfo.Parent != null
                ? Enumerable.Repeat(new FileSystemItem(directoryInfo.Parent, ".."), 1)
                : Enumerable.Empty<FileSystemItem>();

            var items = directoryInfo.EnumerateDirectories().Cast<FileSystemInfo>()
                .Concat(directoryInfo.EnumerateFiles().Cast<FileSystemInfo>())
                .Select(info => new FileSystemItem(info));

            return parentItems
                .Concat(items)
                .ToList();
        }
    }
}
