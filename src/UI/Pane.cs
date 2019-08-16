using System;
using System.Linq;
using PoshCommander.Commands;

namespace PoshCommander.UI
{
    public class Pane : IFileSystemPane
    {
        private DirectoryContents currentDirectory;
        private readonly IExternalApplicationRunner externalApplicationRunner;
        private readonly IFileSystem fileSystem;

        public string CurrentDirectoryPath => currentDirectory.Path;
        public Option<string> Filter { get; private set; } = Option.None;

        private PaneState stateValue;
        public PaneState State
        {
            get => stateValue;
            set => ChangePaneState(value);
        }

        public IPaneView View { get; }

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
            this.View = view;
            this.View.PaneState = State;
            ChangeDirectory(directoryPath, redraw: true);
        }

        public void CreateDirectory(string name)
        {
            // TODO: CreateDirectory(string name)
        }

        public bool ProcessKey(ConsoleKeyInfo keyInfo)
            => ProcessCursorMovementKey(keyInfo)
            || ProcessFilterKey(keyInfo)
            || ProcessNavigationKey(keyInfo)
            || ProcessSelectionKey(keyInfo)
            || ProcessCommandKey(keyInfo);

        private bool ProcessCommandKey(ConsoleKeyInfo keyInfo)
        {
            if (keyInfo.Key == ConsoleKey.F7)
            {
                var command = new CreateDirectoryCommand(
                    new InputReader(
                        new ConsoleReadKeyWrapper()));
                command.Execute(this);
            }
            else
            {
                return false;
            }

            return true;
        }

        private bool ProcessCursorMovementKey(ConsoleKeyInfo keyInfo)
        {
            var selectItems = keyInfo.Modifiers.HasFlag(ConsoleModifiers.Shift);

            if (keyInfo.Key == ConsoleKey.UpArrow)
                SetHighlightedIndex(View.HighlightedIndex - 1, selectItems);
            else if (keyInfo.Key == ConsoleKey.DownArrow)
                SetHighlightedIndex(View.HighlightedIndex + 1, selectItems);
            else if (keyInfo.Key == ConsoleKey.PageUp)
                SetHighlightedIndex(View.HighlightedIndex - View.MaxVisibleItemCount + 1, selectItems);
            else if (keyInfo.Key == ConsoleKey.PageDown)
                SetHighlightedIndex(View.HighlightedIndex + View.MaxVisibleItemCount - 1, selectItems);
            else if (keyInfo.Key == ConsoleKey.Home)
                SetHighlightedIndex(0, selectItems);
            else if (keyInfo.Key == ConsoleKey.End)
                SetHighlightedIndex(View.Items.Count - 1, selectItems);
            else
                return false;

            ScrollToHighlightedItem();
            View.DrawItems();
            return true;
        }

        private void SetHighlightedIndex(int desiredIndex, bool selectItems)
        {
            if (desiredIndex < 0)
                desiredIndex = 0;
            else if (desiredIndex > View.Items.Count - 1)
                desiredIndex = View.Items.Count - 1;

            if (selectItems)
            {
                var wasSelected = View.IsItemSelected(View.GetHighlightedItem());
                var index = View.HighlightedIndex;

                if (desiredIndex < index)
                {
                    do
                    {
                        var item = View.Items[index];
                        if (wasSelected)
                            View.SelectedItems.Remove(item);
                        else
                            View.SelectedItems.Add(item);

                        index--;
                    } while (index > desiredIndex);
                }
                else
                {
                    do
                    {
                        var item = View.Items[index];
                        if (wasSelected)
                            View.SelectedItems.Remove(item);
                        else
                            View.SelectedItems.Add(item);

                        index++;
                    } while (index < desiredIndex);
                }
            }

            View.HighlightedIndex = desiredIndex;
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
                var highlightedItem = View.Items[View.HighlightedIndex];
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
                var highlightedItem = View.Items[View.HighlightedIndex];
                if (highlightedItem.Kind == FileSystemItemKind.File)
                {
                    externalApplicationRunner.RunViewer(highlightedItem.FullPath);
                }
            }
            else if (keyInfo.Key == ConsoleKey.F4)
            {
                var highlightedItem = View.Items[View.HighlightedIndex];
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
                var highlightedItem = View.GetHighlightedItem();
                if (View.IsItemSelected(highlightedItem))
                    View.SelectedItems.Remove(highlightedItem);
                else
                    View.SelectedItems.Add(highlightedItem);
            }
            else if (keyInfo.Key == ConsoleKey.A
                && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                View.SelectedItems.Set(View.Items);
            }
            else if (keyInfo.Key == ConsoleKey.D
                && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                View.SelectedItems.Clear();
            }
            else if (keyInfo.Key == ConsoleKey.I
                && keyInfo.Modifiers.HasFlag(ConsoleModifiers.Control))
            {
                var originalSelection = View.SelectedItems.ToList();
                var newSelection = View.Items
                    .Where(item => !originalSelection.Contains(item));

                View.SelectedItems.Set(newSelection);
            }
            else
            {
                return false;
            }

            View.DrawItems();
            return true;
        }

        private void ScrollToHighlightedItem()
        {
            if (View.HighlightedIndex < View.FirstVisibleItemIndex)
                View.FirstVisibleItemIndex = View.HighlightedIndex;
            else if (View.HighlightedIndex >= View.FirstVisibleItemIndex + View.MaxVisibleItemCount)
                View.FirstVisibleItemIndex = View.HighlightedIndex - View.MaxVisibleItemCount + 1;
        }

        private void SetFilter(Option<string> newFilter)
        {
            var highlightedItem = View.Items[View.HighlightedIndex];

            if (newFilter.HasValue)
            {
                var filteredItems = currentDirectory.Items
                    .Where(item => item.Name.IndexOf(newFilter.Value, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    .ToList();

                if (filteredItems.Count > 0)
                {
                    Filter = newFilter;
                    View.Items = filteredItems;
                    View.StatusText = $"Filter: {Filter}";
                }
            }
            else
            {
                Filter = newFilter;
                View.Items = currentDirectory.Items;
                View.StatusText = FormatStatusText();
            }

            View.SelectedItems.RemoveWhere(item => !View.Items.Contains(item));

            View.HighlightedIndex = View.Items
                .FirstIndexOf(highlightedItem)
                ?? 0;

            ScrollToHighlightedItem();
            View.Redraw();
        }

        private void ChangePaneState(PaneState newState)
        {
            stateValue = newState;
            View.PaneState = State;
            View.Redraw();
        }

        private void ChangeDirectory(string directoryPath, bool redraw)
        {
            var previousDirectory = currentDirectory;

            currentDirectory = fileSystem.GetDirectoryContents(directoryPath);
            Filter = new Option<string>();

            if (previousDirectory != null)
            {
                View.HighlightedIndex = currentDirectory.Items
                    .FirstIndexOf(item => string.Equals(item.FullPath, previousDirectory.Path, StringComparison.OrdinalIgnoreCase))
                    .GetValueOrDefault(0);
            }
            else
            {
                View.HighlightedIndex = 0;
            }

            View.Items = currentDirectory.Items;
            View.SelectedItems.Clear();
            View.StatusText = FormatStatusText();
            View.Title = CurrentDirectoryPath;

            ScrollToHighlightedItem();

            if (redraw)
                View.Redraw();
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
