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

    public class Pane
    {
        private static readonly ConsoleTextStyle titleBarStyle
            = new ConsoleTextStyle(ConsoleColor.DarkGray, ConsoleColor.Yellow);
        private static readonly ConsoleTextStyle itemStyle
            = new ConsoleTextStyle(ConsoleColor.Black, ConsoleColor.White);

        private readonly Rectangle bounds;
        private readonly string directoryPath;
        private readonly IReadOnlyList<Item> items;
        private readonly PSHostUserInterface ui;

        public Pane(string directoryPath, Rectangle bounds, PSHostUserInterface ui)
        {
            this.bounds = bounds;
            this.directoryPath = directoryPath;
            this.items = CreateItemList(directoryPath);
            this.ui = ui;

            Redraw();
        }

        public void ProcessKey(ConsoleKeyInfo keyInfo)
        {
        }

        private void Redraw()
        {
            DrawTitleBar();
            DrawItems();
        }

        private void DrawItems()
        {
            var visibleCount = bounds.GetHeight() - 2;

            for (var i = 0; i < visibleCount; i++)
            {
                var pos = new Coordinates(bounds.Left, bounds.Top + i + 1);
                ui.WriteAt(items[i].Name, pos, itemStyle);
            }
        }

        private void DrawTitleBar()
        {
            var text = directoryPath;
            text += new string(' ', bounds.GetWidth() - directoryPath.Length);
            ui.WriteAt(text, bounds.GetTopLeft(), titleBarStyle);
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
