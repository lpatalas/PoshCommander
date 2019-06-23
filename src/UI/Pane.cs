using System;
using System.Linq;

namespace PoshCommander.UI
{
    public class Pane
    {
        private DirectoryContents currentDirectory;
        private readonly IExternalApplicationRunner externalApplicationRunner;
        private readonly IFileSystem fileSystem;
        private readonly IPaneView view;

        public string CurrentDirectoryPath => currentDirectory.Path;

        public Option<string> Filter { get; private set; } = Option.None;

        private PaneState stateValue;
        public PaneState State
        {
            get => stateValue;
            set => ChangePaneState(value);
        }

        public Pane(
            string directoryPath,
            IExternalApplicationRunner externalApplicationRunner,
            IFileSystem fileSystem,
            PaneState paneState,
            IPaneView view)
        {
            this.externalApplicationRunner = externalApplicationRunner;
            this.fileSystem = fileSystem;
            this.stateValue = paneState;
            this.view = view;
            this.view.PaneState = State;
            ChangeDirectory(directoryPath, redraw: true);
        }

        public bool ProcessKey(ConsoleKeyInfo keyInfo)
            => ProcessCursorMovementKey(keyInfo)
            || ProcessFilterKey(keyInfo)
            || ProcessNavigationKey(keyInfo)
            || ProcessSelectionKey(keyInfo);

        private bool ProcessCursorMovementKey(ConsoleKeyInfo keyInfo)
        {
            var selectItems = keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift);

            if (keyInfo.Key == ConsoleKey.UpArrow)
                SetHighlightedIndex(view.HighlightedIndex - 1, selectItems);
            else if (keyInfo.Key == ConsoleKey.DownArrow)
                SetHighlightedIndex(view.HighlightedIndex + 1, selectItems);
            else if (keyInfo.Key == ConsoleKey.PageUp)
                SetHighlightedIndex(view.HighlightedIndex - view.MaxVisibleItemCount + 1, selectItems);
            else if (keyInfo.Key == ConsoleKey.PageDown)
                SetHighlightedIndex(view.HighlightedIndex + view.MaxVisibleItemCount - 1, selectItems);
            else if (keyInfo.Key == ConsoleKey.Home)
                SetHighlightedIndex(0, selectItems);
            else if (keyInfo.Key == ConsoleKey.End)
                SetHighlightedIndex(view.Items.Count - 1, selectItems);
            else
                return false;

            ScrollToHighlightedItem();
            view.DrawItems();
            return true;
        }

        private void SetHighlightedIndex(int desiredIndex, bool selectItems)
        {
            if (desiredIndex < 0)
                desiredIndex = 0;
            else if (desiredIndex > view.Items.Count - 1)
                desiredIndex = view.Items.Count - 1;

            if (selectItems)
            {
                var wasSelected = view.IsItemSelected(view.GetHighlightedItem());
                var index = view.HighlightedIndex;

                if (desiredIndex < index)
                {
                    do
                    {
                        var item = view.Items[index];
                        if (wasSelected)
                            view.SelectedItems.Remove(item);
                        else
                            view.SelectedItems.Add(item);

                        index--;
                    } while (index > desiredIndex);
                }
                else
                {
                    do
                    {
                        var item = view.Items[index];
                        if (wasSelected)
                            view.SelectedItems.Remove(item);
                        else
                            view.SelectedItems.Add(item);

                        index++;
                    } while (index < desiredIndex);
                }
            }

            view.HighlightedIndex = desiredIndex;
        }

        private bool ProcessFilterKey(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
                return false;

            if (keyInfo.Key >= ConsoleKey.A && keyInfo.Key <= ConsoleKey.Z)
            {
                SetFilter(Filter
                    .DefaultIfEmpty(string.Empty)
                    .Map(text => text + keyInfo.KeyChar));
            }
            else if (keyInfo.Key == ConsoleKey.Backspace
                && Filter.HasValue)
            {
                if (Filter.Value.Length > 0)
                {
                    SetFilter(
                        Filter.Map(text
                            => text.Substring(0, text.Length - 1)));
                }
            }
            else if (keyInfo.Key == ConsoleKey.Escape
                && Filter.HasValue)
            {
                SetFilter(Option.None);
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
                else if (highlightedItem.Kind == FileSystemItemKind.File)
                {
                    externalApplicationRunner.RunAssociatedApplication(highlightedItem.FullPath);
                    return true;
                }
            }
            else if (keyInfo.Key == ConsoleKey.Backspace)
            {
                var parentItem = currentDirectory.Items
                    .FirstOrDefault(item => item.Kind == FileSystemItemKind.ParentDirectory);

                if (parentItem != null)
                {
                    ChangeDirectory(parentItem.FullPath, true);
                    return true;
                }
            }
            else if (keyInfo.Key == ConsoleKey.F3)
            {
                var highlightedItem = view.Items[view.HighlightedIndex];
                if (highlightedItem.Kind == FileSystemItemKind.File)
                {
                    externalApplicationRunner.RunViewer(highlightedItem.FullPath);
                }
            }
            else if (keyInfo.Key == ConsoleKey.F4)
            {
                var highlightedItem = view.Items[view.HighlightedIndex];
                if (highlightedItem.Kind == FileSystemItemKind.File)
                {
                    externalApplicationRunner.RunEditor(highlightedItem.FullPath);
                }
            }

            return false;
        }

        private bool ProcessSelectionKey(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.Spacebar)
            {
                var highlightedItem = view.GetHighlightedItem();
                if (view.IsItemSelected(highlightedItem))
                    view.SelectedItems.Remove(highlightedItem);
                else
                    view.SelectedItems.Add(highlightedItem);
            }
            else if (keyInfo.Key == ConsoleKey.A
                && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                view.SelectedItems.Set(view.Items);
            }
            else if (keyInfo.Key == ConsoleKey.D
                && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                view.SelectedItems.Clear();
            }
            else if (keyInfo.Key == ConsoleKey.I
                && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                var originalSelection = view.SelectedItems.ToList();
                var newSelection = view.Items
                    .Where(item => !originalSelection.Contains(item));

                view.SelectedItems.Set(newSelection);
            }
            else
            {
                return false;
            }

            view.DrawItems();
            return true;
        }

        private void ScrollToHighlightedItem()
        {
            if (view.HighlightedIndex < view.FirstVisibleItemIndex)
                view.FirstVisibleItemIndex = view.HighlightedIndex;
            else if (view.HighlightedIndex >= view.FirstVisibleItemIndex + view.MaxVisibleItemCount)
                view.FirstVisibleItemIndex = view.HighlightedIndex - view.MaxVisibleItemCount + 1;
        }

        private void SetFilter(Option<string> newFilter)
        {
            var highlightedItem = view.Items[view.HighlightedIndex];

            if (newFilter.HasValue)
            {
                var filteredItems = currentDirectory.Items
                    .Where(item => item.Name.IndexOf(newFilter.Value, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .ToList();

                if (filteredItems.Count > 0)
                {
                    Filter = newFilter;
                    view.Items = filteredItems;
                    view.StatusText = $"Filter: {Filter}";
                }
            }
            else
            {
                Filter = newFilter;
                view.Items = currentDirectory.Items;
                view.StatusText = FormatStatusText();
            }

            view.SelectedItems.RemoveWhere(item => !view.Items.Contains(item));

            view.HighlightedIndex = view.Items
                .FirstIndexOf(highlightedItem)
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
            var previousDirectory = currentDirectory;

            currentDirectory = fileSystem.GetDirectoryContents(directoryPath);
            Filter = new Option<string>();

            if (previousDirectory != null)
            {
                view.HighlightedIndex = currentDirectory.Items
                    .FirstIndexOf(item => string.Equals(item.FullPath, previousDirectory.Path, StringComparison.OrdinalIgnoreCase))
                    .GetValueOrDefault(0);
            }
            else
            {
                view.HighlightedIndex = 0;
            }

            view.Items = currentDirectory.Items;
            view.SelectedItems.Clear();
            view.StatusText = FormatStatusText();
            view.Title = CurrentDirectoryPath;

            ScrollToHighlightedItem();

            if (redraw)
                view.Redraw();
        }

        private string FormatStatusText()
        {
            if (currentDirectory.IsAccessAllowed)
            {
                var fileCount = currentDirectory.Items
                    .Count(item => item.Kind == FileSystemItemKind.File);
                var directoryCount = currentDirectory.Items
                    .Count(item => item.Kind == FileSystemItemKind.Directory);
                return $"Files: {fileCount}, Directories: {directoryCount}";
            }
            else
            {
                return "ACCESS DENIED";
            }
        }
    }
}
