using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Host;
using System.Text;

namespace PoshCommander
{
    public enum ItemKind
    {
        Directory,
        File
    }

    public class Item
    {
        public string FullPath { get; }
        public ItemKind Kind { get; }
        public string Name { get; }

        public Item(ItemKind kind, string fullPath)
        {
            Kind = kind;
            FullPath = fullPath;
            Name = Path.GetFileName(fullPath);
        }
    }

    public enum PaneState
    {
        Inactive,
        Active
    }

    public class Pane
    {
        private static readonly ConsoleTextStyle itemStyle
            = new ConsoleTextStyle(ConsoleColor.Black, ConsoleColor.White);
        private static readonly ConsoleTextStyle itemStyleHighlighted
            = new ConsoleTextStyle(ConsoleColor.DarkRed, ConsoleColor.White);
        private static readonly ConsoleTextStyle statusBarStyle
            = new ConsoleTextStyle(ConsoleColor.DarkGray, ConsoleColor.Black);
        private static readonly ConsoleTextStyle titleBarStyleActive
            = new ConsoleTextStyle(ConsoleColor.DarkBlue, ConsoleColor.Yellow);
        private static readonly ConsoleTextStyle titleBarStyleInactive
            = new ConsoleTextStyle(ConsoleColor.DarkGray, ConsoleColor.Yellow);

        private readonly Rectangle bounds;
        private readonly string directoryPath;
        private int firstVisibleItemIndex;
        private int highlightedIndex;
        private readonly IReadOnlyList<Item> items;
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
            this.directoryPath = directoryPath;
            this.State = paneState;
            this.items = CreateItemList(directoryPath);
            this.maxVisibleItemCount = bounds.GetHeight() - 2;
            this.ui = ui;
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

            if (highlightedIndex < firstVisibleItemIndex)
                firstVisibleItemIndex = highlightedIndex;
            else if (highlightedIndex >= firstVisibleItemIndex + maxVisibleItemCount)
                firstVisibleItemIndex = highlightedIndex - maxVisibleItemCount + 1;

            DrawItems();
        }

        public void Redraw()
        {
            DrawTitleBar();
            DrawItems();
            DrawStatusBar();
        }

        private void DrawItems()
        {
            var visibleCount = Math.Min(items.Count, maxVisibleItemCount);

            for (var i = 0; i < visibleCount; i++)
            {
                var pos = new Coordinates(bounds.Left, bounds.Top + i + 1);
                var itemIndex = i + firstVisibleItemIndex;
                ui.WriteBlockAt(
                    items[itemIndex].Name,
                    pos,
                    bounds.GetWidth(),
                    itemIndex == highlightedIndex
                        ? itemStyleHighlighted
                        : itemStyle);
            }
        }

        private void DrawStatusBar()
        {
            var fileCount = items.Count(item => item.Kind == ItemKind.File);
            var directoryCount = items.Count(item => item.Kind == ItemKind.Directory);
            var text = $"Files: {fileCount}, Directories: {directoryCount}";
            ui.WriteBlockAt(text, bounds.GetBottomLeft(), bounds.GetWidth(), statusBarStyle);
        }

        private void DrawTitleBar()
        {
            ui.WriteBlockAt(
                directoryPath,
                bounds.GetTopLeft(),
                bounds.GetWidth(),
                State == PaneState.Active
                    ? titleBarStyleActive
                    : titleBarStyleInactive);
        }

        private static IReadOnlyList<Item> CreateItemList(string directoryPath)
        {
            var directories = Directory.EnumerateDirectories(directoryPath)
                .Select(path => new Item(ItemKind.Directory, path));
            var files = Directory.EnumerateFiles(directoryPath)
                .Select(path => new Item(ItemKind.File, path));

            return directories
                .Concat(files)
                .ToList();
        }
    }
}
