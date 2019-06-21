using System;
using System.Collections.Generic;
using System.Text;
using FluentAssertions;
using Xunit;

namespace PoshCommander.Tests
{
    public class PaneNavigationTests
    {
        [Fact]
        public void When_Backspace_is_pressed_it_should_go_to_parent_directory()
        {
            // Arrange
            var view = new FakePaneView();

            var fileSystem = new StubFileSystem(new[]
            {
                new FileSystemItem(@"X:\A", FileSystemItemKind.ParentDirectory, ".."),
                new FileSystemItem(@"X:\A\B\C", FileSystemItemKind.Directory, "C"),
                new FileSystemItem(@"X:\A\B\D.txt", FileSystemItemKind.Directory, "D.txt"),
            });

            var pane = new Pane(@"X:\A\B", fileSystem, PaneState.Active, view);

            // Act
            pane.ProcessKey(ConsoleKey.Backspace.ToKeyInfo());

            // Assert
            pane.CurrentDirectoryPath.Should().Be(@"X:\A");
        }

        [Fact]
        public void When_Backspace_is_pressed_and_current_directory_has_no_parent_it_should_stay_in_current_directory()
        {
            // Arrange
            var view = new FakePaneView();

            var fileSystem = new StubFileSystem(new[]
            {
                new FileSystemItem(@"X:\A\B\C", FileSystemItemKind.Directory, "C"),
                new FileSystemItem(@"X:\A\B\D.txt", FileSystemItemKind.Directory, "D.txt"),
            });

            var pane = new Pane(@"X:\A\B", fileSystem, PaneState.Active, view);

            // Act
            pane.ProcessKey(ConsoleKey.Backspace.ToKeyInfo());

            // Assert
            pane.CurrentDirectoryPath.Should().Be(@"X:\A\B");
        }

        [Fact]
        public void When_Enter_is_pressed_while_directory_is_highlighted_it_should_change_current_directory_to_highlighted_item()
        {
            // Arrange
            var view = new FakePaneView();

            var fileSystem = new StubFileSystem(new[]
            {
                new FileSystemItem(@"X:\A", FileSystemItemKind.Directory, "A"),
                new FileSystemItem(@"X:\B", FileSystemItemKind.Directory, "B"),
                new FileSystemItem(@"X:\C", FileSystemItemKind.Directory, "C"),
            });

            var pane = new Pane(@"X:", fileSystem, PaneState.Active, view);

            view.HighlightedIndex = 1;

            // Act
            pane.ProcessKey(ConsoleKey.Enter.ToKeyInfo());

            // Assert
            pane.CurrentDirectoryPath.Should().Be(@"X:\B");
        }

        [Fact]
        public void When_Enter_is_pressed_while_parent_directory_is_highlighted_it_should_change_current_directory_to_parent_directory()
        {
            // Arrange
            var view = new FakePaneView();

            var fileSystem = new StubFileSystem(new[]
            {
                new FileSystemItem(@"X:\A", FileSystemItemKind.ParentDirectory, ".."),
                new FileSystemItem(@"X:\A\B\C", FileSystemItemKind.Directory, "C"),
                new FileSystemItem(@"X:\A\B\D.txt", FileSystemItemKind.Directory, "D.txt"),
            });

            var pane = new Pane(@"X:\A\B", fileSystem, PaneState.Active, view);

            view.HighlightedIndex = 0;

            // Act
            pane.ProcessKey(ConsoleKey.Enter.ToKeyInfo());

            // Assert
            pane.CurrentDirectoryPath.Should().Be(@"X:\A");
        }

        [Fact]
        public void When_current_directory_is_changed_to_parent_directory_it_should_highlight_previous_directory_on_list()
        {
            // Arrange
            var view = new FakePaneView();

            var parentDirectory = @"X:\A";
            var childDirectory = @"X:\A\C";

            var fileSystem = new FakeFileSystem();
            fileSystem.Directories.Add(parentDirectory, new[]
            {
                new FileSystemItem(@"X:", FileSystemItemKind.ParentDirectory, ".."),
                new FileSystemItem(@"X:\A\B", FileSystemItemKind.Directory, "B"),
                new FileSystemItem($"{childDirectory}", FileSystemItemKind.Directory, "C"),
                new FileSystemItem(@"X:\A\D", FileSystemItemKind.Directory, "D"),
            });
            fileSystem.Directories.Add(childDirectory, new[]
            {
                new FileSystemItem(parentDirectory, FileSystemItemKind.ParentDirectory, ".."),
                new FileSystemItem(@"X:\A\C\Z", FileSystemItemKind.Directory, "Z"),
            });

            var pane = new Pane(@"X:\A\C", fileSystem, PaneState.Active, view);

            // Act
            pane.ProcessKey(ConsoleKey.Backspace.ToKeyInfo());

            // Assert
            var expectedIndex = fileSystem.Directories[parentDirectory]
                .FirstIndexOf(item => item.FullPath == childDirectory);
            view.HighlightedIndex.Should().Be(expectedIndex.Value);
        }
    }
}

