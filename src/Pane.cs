﻿using System;
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

        public static Item Directory(string fullPath)
            => Directory(fullPath, Path.GetFileName(fullPath));

        public static Item Directory(string fullPath, string name)
            => new Item(fullPath, ItemKind.Directory, name);

        public static Item File(string fullPath)
            => new Item(fullPath, ItemKind.File, Path.GetFileName(fullPath));

        private Item(string fullPath, ItemKind kind, string name)
        {
            Kind = kind;
            FullPath = fullPath;
            Name = name;
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
            = new ConsoleTextStyle(new RgbColor(29, 40, 61), RgbColor.White);
        private static readonly ConsoleTextStyle itemStyleHighlighted
            = new ConsoleTextStyle(new RgbColor(100, 100, 100), RgbColor.White);
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
        private IReadOnlyList<Item> items;
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
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                var highlightedItem = items[highlightedIndex];
                if (highlightedItem.Kind == ItemKind.Directory)
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
                var text
                    = itemIndex < items.Count
                    ? items[itemIndex].Name
                    : string.Empty;

                ui.WriteBlockAt(
                    text,
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
                currentDirectoryPath,
                bounds.GetTopLeft(),
                bounds.GetWidth(),
                State == PaneState.Active
                    ? titleBarStyleActive
                    : titleBarStyleInactive);
        }

        private static IReadOnlyList<Item> CreateItemList(string directoryPath)
        {
            var parentInfo = Directory.GetParent(directoryPath);
            var parentItems
                = parentInfo != null
                ? Enumerable.Repeat(Item.Directory(parentInfo.FullName, ".."), 1)
                : Enumerable.Empty<Item>();

            var directories = Directory.EnumerateDirectories(directoryPath)
                .Select(Item.Directory);
            var files = Directory.EnumerateFiles(directoryPath)
                .Select(Item.File);

            return parentItems
                .Concat(directories)
                .Concat(files)
                .ToList();
        }
    }
}
