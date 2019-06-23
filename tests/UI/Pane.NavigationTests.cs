using System;
using FluentAssertions;
using PoshCommander.Tests.TestDoubles;
using PoshCommander.UI;
using Xunit;

namespace PoshCommander.Tests.UI
{
    public class PaneNavigationTests
    {
        private readonly DummyExternalApplicationRunner externalApplicationRunner
            = new DummyExternalApplicationRunner();

        private readonly FileSystemItem parentDirectoryItem
            = new FileSystemItem(@"X:\A", FileSystemItemKind.ParentDirectory, "..");

        private readonly FileSystemItem directoryItem
            = new FileSystemItem(@"X:\A\B\C", FileSystemItemKind.Directory, "C");

        private readonly FileSystemItem fileItem
            = new FileSystemItem(@"X:\A\B\D.txt", FileSystemItemKind.File, "D.txt");

        private readonly FakePaneView view = new FakePaneView();

        private Pane CreatePane(bool includeParentDirectory = true)
        {
            var items = includeParentDirectory
                ? new[] { parentDirectoryItem, directoryItem, fileItem }
                : new[] { directoryItem, fileItem };

            var fileSystem = new StubFileSystem(items);

            return new Pane(
                @"X:\A\B",
                externalApplicationRunner,
                fileSystem,
                PaneState.Active,
                view);
        }

        [Fact]
        public void When_Backspace_is_pressed_it_should_go_to_parent_directory()
        {
            // Arrange
            var pane = CreatePane();

            // Act
            pane.ProcessKey(ConsoleKey.Backspace.ToKeyInfo());

            // Assert
            pane.CurrentDirectoryPath.Should().Be(parentDirectoryItem.FullPath);
        }

        [Fact]
        public void When_Backspace_is_pressed_and_current_directory_has_no_parent_it_should_stay_in_current_directory()
        {
            // Arrange
            var pane = CreatePane(includeParentDirectory: false);
            var originalDirectoryPath = pane.CurrentDirectoryPath;

            // Act
            pane.ProcessKey(ConsoleKey.Backspace.ToKeyInfo());

            // Assert
            pane.CurrentDirectoryPath.Should().Be(originalDirectoryPath);
        }

        [Fact]
        public void When_Enter_is_pressed_while_directory_is_highlighted_it_should_change_current_directory_to_highlighted_item()
        {
            // Arrange
            var pane = CreatePane();
            view.SetHighlightedItem(directoryItem);

            // Act
            pane.ProcessKey(ConsoleKey.Enter.ToKeyInfo());

            // Assert
            pane.CurrentDirectoryPath.Should().Be(directoryItem.FullPath);
        }

        [Fact]
        public void When_Enter_is_pressed_while_parent_directory_is_highlighted_it_should_change_current_directory_to_parent_directory()
        {
            // Arrange
            var pane = CreatePane();
            view.SetHighlightedItem(parentDirectoryItem);

            // Act
            pane.ProcessKey(ConsoleKey.Enter.ToKeyInfo());

            // Assert
            pane.CurrentDirectoryPath.Should().Be(parentDirectoryItem.FullPath);
        }

        [Fact]
        public void When_current_directory_is_changed_to_parent_directory_it_should_highlight_previous_directory_on_list()
        {
            // Arrange
            var view = new FakePaneView();

            var childDirectoryPath = @"X:\A\C";
            var parentDirectoryPath = @"X:\A";

            var parentDirectory = new DirectoryContents(parentDirectoryPath, new[]
            {
                new FileSystemItem(@"X:", FileSystemItemKind.ParentDirectory, ".."),
                new FileSystemItem(@"X:\A\B", FileSystemItemKind.Directory, "B"),
                new FileSystemItem($"{childDirectoryPath}", FileSystemItemKind.Directory, "C"),
                new FileSystemItem(@"X:\A\D", FileSystemItemKind.Directory, "D"),
            });

            var childDirectory = new DirectoryContents(childDirectoryPath, new[]
            {
                new FileSystemItem(parentDirectoryPath, FileSystemItemKind.ParentDirectory, ".."),
                new FileSystemItem(@"X:\A\C\Z", FileSystemItemKind.Directory, "Z"),
            });

            var fileSystem = new FakeFileSystem();
            fileSystem.Directories.Add(parentDirectory);
            fileSystem.Directories.Add(childDirectory);

            var pane = new Pane(@"X:\A\C", externalApplicationRunner, fileSystem, PaneState.Active, view);

            // Act
            pane.ProcessKey(ConsoleKey.Backspace.ToKeyInfo());

            // Assert
            var expectedIndex = parentDirectory.Items
                .FirstIndexOf(item => item.FullPath == childDirectory.Path);
            view.HighlightedIndex.Should().Be(expectedIndex.Value);
        }

        [Fact]
        public void When_access_to_current_directory_is_not_allowed_it_should_show_error_message_in_status_bar()
        {
            // Arrange
            var view = new FakePaneView();
            var fileSystem = new FakeFileSystem();

            fileSystem.Directories.Add(new DirectoryContents(@"X:\A", new[]
            {
                new FileSystemItem(@"X:", FileSystemItemKind.ParentDirectory, ".."),
                new FileSystemItem(@"X:\A\B", FileSystemItemKind.Directory, "B"),
            }));
            fileSystem.Directories.Add(DirectoryContents.AccessNotAllowed(@"X:\A\B", new[]
            {
                new FileSystemItem(@"X:\A", FileSystemItemKind.ParentDirectory, "..")
            }));

            var pane = new Pane(@"X:\A", externalApplicationRunner, fileSystem, PaneState.Active, view);

            view.HighlightedIndex = 1;

            // Act
            pane.ProcessKey(ConsoleKey.Enter.ToKeyInfo());

            // Assert
            view.StatusText.Should().Be("ACCESS DENIED");
        }
    }
}

