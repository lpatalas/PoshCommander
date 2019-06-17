using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Host;

namespace PoshCommander
{
    public class Pane
    {
        private string currentDirectoryPath;
        private IReadOnlyList<FileSystemItem> items;
        private readonly PaneView view;

        private PaneState stateValue;
        public PaneState State
        {
            get => stateValue;
            set => ChangePaneState(value);
        }

        public Pane(
            string directoryPath,
            Rectangle bounds,
            PaneState paneState,
            PSHostUserInterface ui)
        {
            this.stateValue = paneState;
            this.view = new PaneView(bounds, ui)
            {
                PaneState = State
            };

            ChangeDirectory(directoryPath, redraw: true);
        }

        public void ProcessKey(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.UpArrow)
                view.HighlightedIndex = Math.Max(0, view.HighlightedIndex - 1);
            else if (keyInfo.Key == ConsoleKey.DownArrow)
                view.HighlightedIndex = Math.Min(items.Count - 1, view.HighlightedIndex + 1);
            else if (keyInfo.Key == ConsoleKey.PageUp)
                view.HighlightedIndex = Math.Max(0, view.HighlightedIndex - view.MaxVisibleItemCount + 1);
            else if (keyInfo.Key == ConsoleKey.PageDown)
                view.HighlightedIndex = Math.Min(items.Count - 1, view.HighlightedIndex + view.MaxVisibleItemCount - 1);
            else if (keyInfo.Key == ConsoleKey.Home)
                view.HighlightedIndex = 0;
            else if (keyInfo.Key == ConsoleKey.End)
                view.HighlightedIndex = items.Count - 1;
            else if (keyInfo.Key == ConsoleKey.Enter)
            {
                var highlightedItem = items[view.HighlightedIndex];
                if (highlightedItem.Kind == FileSystemItemKind.Directory
                    || highlightedItem.Kind == FileSystemItemKind.ParentDirectory)
                {
                    ChangeDirectory(highlightedItem.FullPath, true);
                    return;
                }
            }

            view.ScrollToHighlightedItem();
            view.DrawItems();
        }

        private void ChangePaneState(PaneState newState)
        {
            stateValue = newState;
            view.PaneState = State;
            view.Redraw();
        }

        private void ChangeDirectory(string directoryPath, bool redraw)
        {
            var previousDirectoryPath = currentDirectoryPath;
            currentDirectoryPath = directoryPath;
            items = CreateItemList(directoryPath);

            view.HighlightedIndex = items
                .FirstIndexOf(item => string.Equals(item.FullPath, previousDirectoryPath, StringComparison.OrdinalIgnoreCase))
                .GetValueOrDefault(0);

            view.Items = items;
            view.StatusText = FormatStatusText();
            view.Title = currentDirectoryPath;
            view.ScrollToHighlightedItem();

            if (redraw)
                view.Redraw();
        }

        private string FormatStatusText()
        {
            var fileCount = items.Count(item => item.Kind == FileSystemItemKind.File);
            var directoryCount = items.Count(item => item.Kind == FileSystemItemKind.Directory);
            return $"Files: {fileCount}, Directories: {directoryCount}";
        }

        private static IReadOnlyList<FileSystemItem> CreateItemList(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            var parentItems
                = directoryInfo.Parent != null
                ? Enumerable.Repeat(FileSystemItem.CreateParentDirectory(directoryInfo.Parent), 1)
                : Enumerable.Empty<FileSystemItem>();

            var items = directoryInfo.EnumerateDirectories().Cast<FileSystemInfo>()
                .Concat(directoryInfo.EnumerateFiles().Cast<FileSystemInfo>())
                .Select(FileSystemItem.FromFileSystemInfo);

            return parentItems
                .Concat(items)
                .ToList();
        }
    }
}
