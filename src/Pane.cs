using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation.Host;

namespace PoshCommander
{
    public class Pane
    {
        private readonly IFileSystem fileSystem;
        private string filter = string.Empty;
        private bool isFilterActive;
        private IReadOnlyList<FileSystemItem> items;
        private readonly IPaneView view;

        public string CurrentDirectoryPath { get; private set; }

        private PaneState stateValue;
        public PaneState State
        {
            get => stateValue;
            set => ChangePaneState(value);
        }

        public Pane(
            string directoryPath,
            IFileSystem fileSystem,
            PaneState paneState,
            IPaneView view)
        {
            this.fileSystem = fileSystem;
            this.stateValue = paneState;
            this.view = view;
            this.view.PaneState = State;
            ChangeDirectory(directoryPath, redraw: true);
        }

        public bool ProcessKey(ConsoleKeyInfo keyInfo)
            => ProcessCursorMovementKey(keyInfo)
            || ProcessFilterKey(keyInfo)
            || ProcessNavigationKey(keyInfo);

        private bool ProcessCursorMovementKey(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.UpArrow)
                view.HighlightedIndex = Math.Max(0, view.HighlightedIndex - 1);
            else if (keyInfo.Key == ConsoleKey.DownArrow)
                view.HighlightedIndex = Math.Min(view.Items.Count - 1, view.HighlightedIndex + 1);
            else if (keyInfo.Key == ConsoleKey.PageUp)
                view.HighlightedIndex = Math.Max(0, view.HighlightedIndex - view.MaxVisibleItemCount + 1);
            else if (keyInfo.Key == ConsoleKey.PageDown)
                view.HighlightedIndex = Math.Min(view.Items.Count - 1, view.HighlightedIndex + view.MaxVisibleItemCount - 1);
            else if (keyInfo.Key == ConsoleKey.Home)
                view.HighlightedIndex = 0;
            else if (keyInfo.Key == ConsoleKey.End)
                view.HighlightedIndex = view.Items.Count - 1;
            else
                return false;

            ScrollToHighlightedItem();
            view.DrawItems();
            return true;
        }

        private bool ProcessFilterKey(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key >= ConsoleKey.A && keyInfo.Key <= ConsoleKey.Z)
            {
                isFilterActive = true;
                SetFilter(filter + keyInfo.KeyChar);
            }
            else if (keyInfo.Key == ConsoleKey.Backspace
                && isFilterActive)
            {
                if (filter.Length > 0)
                    SetFilter(filter.Substring(0, filter.Length - 1));
            }
            else if (keyInfo.Key == ConsoleKey.Escape
                && isFilterActive)
            {
                isFilterActive = false;
                SetFilter(string.Empty);
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool ProcessNavigationKey(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.Enter)
            {
                var highlightedItem = view.Items[view.HighlightedIndex];
                if (highlightedItem.Kind == FileSystemItemKind.Directory
                    || highlightedItem.Kind == FileSystemItemKind.ParentDirectory)
                {
                    ChangeDirectory(highlightedItem.FullPath, true);
                    return true;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                var parentItem = items
                    .FirstOrDefault(item => item.Kind == FileSystemItemKind.ParentDirectory);

                if (parentItem != null)
                {
                    ChangeDirectory(parentItem.FullPath, true);
                    return true;
                }
            }

            return false;
        }

        private void ScrollToHighlightedItem()
        {
            if (view.HighlightedIndex < view.FirstVisibleItemIndex)
                view.FirstVisibleItemIndex = view.HighlightedIndex;
            else if (view.HighlightedIndex >= view.FirstVisibleItemIndex + view.MaxVisibleItemCount)
                view.FirstVisibleItemIndex = view.HighlightedIndex - view.MaxVisibleItemCount + 1;
        }

        private void SetFilter(string newFilter)
        {
            var highlightedItem = view.Items[view.HighlightedIndex];

            if (isFilterActive)
            {
                var filteredItems = items
                    .Where(item => item.Name.IndexOf(newFilter, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .ToList();

                if (filteredItems.Count > 0)
                {
                    filter = newFilter;
                    view.Items = filteredItems;
                    view.StatusText = $"Filter: {filter}";
                }
            }
            else
            {
                view.Items = items;
                view.StatusText = FormatStatusText();
            }

            view.HighlightedIndex = view.Items
                .FirstIndexOf(item => ReferenceEquals(item, highlightedItem))
                ?? 0;

            ScrollToHighlightedItem();
            view.Redraw();
        }

        private void ChangePaneState(PaneState newState)
        {
            stateValue = newState;
            view.PaneState = State;
            view.Redraw();
        }

        private void ChangeDirectory(string directoryPath, bool redraw)
        {
            var previousDirectoryPath = CurrentDirectoryPath;

            CurrentDirectoryPath = directoryPath;
            filter = string.Empty;
            isFilterActive = false;
            items = fileSystem.GetChildItems(directoryPath);

            view.HighlightedIndex = items
                .FirstIndexOf(item => string.Equals(item.FullPath, previousDirectoryPath, StringComparison.OrdinalIgnoreCase))
                .GetValueOrDefault(0);

            view.Items = items;
            view.StatusText = FormatStatusText();
            view.Title = CurrentDirectoryPath;

            ScrollToHighlightedItem();

            if (redraw)
                view.Redraw();
        }

        private IReadOnlyList<FileSystemItem> GetFilteredItems()
        {
            if (!string.IsNullOrEmpty(filter))
                return items.Where(item => item.Name.Contains(filter)).ToList();
            else
                return items;
        }

        private string FormatStatusText()
        {
            var fileCount = items.Count(item => item.Kind == FileSystemItemKind.File);
            var directoryCount = items.Count(item => item.Kind == FileSystemItemKind.Directory);
            return $"Files: {fileCount}, Directories: {directoryCount}";
        }
    }
}
