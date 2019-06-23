using System;
using FluentAssertions;
using PoshCommander.Tests.TestDoubles;
using PoshCommander.UI;
using Xunit;

namespace PoshCommander.Tests.UI
{
    public class PaneFileExecutionTests
    {
        private readonly FileSystemItem fileItem
            = new FileSystemItem(@"X:\A.txt", FileSystemItemKind.File, "A.txt");

        private readonly FileSystemItem directoryItem
            = new FileSystemItem(@"X:\B", FileSystemItemKind.Directory, "B");

        private readonly SpyExternalApplicationRunner spyExternalApplicationRunner
            = new SpyExternalApplicationRunner();

        private readonly FakePaneView view = new FakePaneView();

        private readonly Pane pane;

        public PaneFileExecutionTests()
        {
            var fileSystem = new StubFileSystem(new[]
            {
                fileItem,
                directoryItem
            });

            pane = new Pane(
                @"X:",
                spyExternalApplicationRunner,
                fileSystem,
                PaneState.Active,
                view);

            view.HighlightedIndex = 0;
        }

        [Fact]
        public void When_Enter_is_pressed_on_highlighted_file_it_should_run_external_application()
        {
            // Act
            pane.ProcessKey(ConsoleKey.Enter.ToKeyInfo());

            // Assert
            spyExternalApplicationRunner.ExecutedFiles.Should().ContainInOrder(fileItem.FullPath);
        }

        [Fact]
        public void When_F3_is_pressed_on_highlighted_file_it_should_run_external_viewer()
        {
            // Act
            pane.ProcessKey(ConsoleKey.F3.ToKeyInfo());

            // Assert
            spyExternalApplicationRunner.ViewedFiles.Should().ContainInOrder(fileItem.FullPath);
        }

        [Fact]
        public void When_F3_is_pressed_on_highlighted_directory_it_should_not_run_external_viewer()
        {
            // Arrange
            view.SetHighlightedItem(directoryItem);

            // Act
            pane.ProcessKey(ConsoleKey.F3.ToKeyInfo());

            // Assert
            spyExternalApplicationRunner.ViewedFiles.Should().BeEmpty();
        }

        [Fact]
        public void When_F4_is_pressed_on_highlighted_file_it_should_run_external_editor()
        {
            // Act
            pane.ProcessKey(ConsoleKey.F4.ToKeyInfo());

            // Assert
            spyExternalApplicationRunner.EditedFiles.Should().ContainInOrder(fileItem.FullPath);
        }

        [Fact]
        public void When_F4_is_pressed_on_highlighted_directory_it_should_not_run_external_editor()
        {
            // Arrange
            view.SetHighlightedItem(directoryItem);

            // Act
            pane.ProcessKey(ConsoleKey.F4.ToKeyInfo());

            // Assert
            spyExternalApplicationRunner.EditedFiles.Should().BeEmpty();
        }
    }
}
