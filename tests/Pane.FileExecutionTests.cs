using System;
using FluentAssertions;
using Xunit;

namespace PoshCommander.Tests
{
    public class PaneFileExecutionTests
    {
        [Fact]
        public void When_Enter_is_pressed_on_highlighted_file_it_should_run_external_application()
        {
            // Arrange
            var spyExternalApplicationRunner = new SpyExternalApplicationRunner();
            var view = new FakePaneView();
            var file = new FileSystemItem(@"X:\A.txt", FileSystemItemKind.File, "A.txt");
            var fileSystem = new StubFileSystem(new[] { file });

            var pane = new Pane(
                @"X:",
                spyExternalApplicationRunner,
                fileSystem,
                PaneState.Active,
                view);

            view.HighlightedIndex = 0;

            // Act
            pane.ProcessKey(ConsoleKey.Enter.ToKeyInfo());

            // Assert
            spyExternalApplicationRunner.ExecutedFiles.Should().ContainInOrder(file.FullPath);
        }

        [Fact]
        public void When_F3_is_pressed_on_highlighted_file_it_should_run_external_viewer()
        {
            // Arrange
            var spyExternalApplicationRunner = new SpyExternalApplicationRunner();
            var view = new FakePaneView();
            var file = new FileSystemItem(@"X:\A.txt", FileSystemItemKind.File, "A.txt");
            var fileSystem = new StubFileSystem(new[] { file });

            var pane = new Pane(
                @"X:",
                spyExternalApplicationRunner,
                fileSystem,
                PaneState.Active,
                view);

            view.HighlightedIndex = 0;

            // Act
            pane.ProcessKey(ConsoleKey.F3.ToKeyInfo());

            // Assert
            spyExternalApplicationRunner.ViewedFiles.Should().ContainInOrder(file.FullPath);
        }

        [Fact]
        public void When_F4_is_pressed_on_highlighted_file_it_should_run_external_editor()
        {
            // Arrange
            var spyExternalApplicationRunner = new SpyExternalApplicationRunner();
            var view = new FakePaneView();
            var file = new FileSystemItem(@"X:\A.txt", FileSystemItemKind.File, "A.txt");
            var fileSystem = new StubFileSystem(new[] { file });

            var pane = new Pane(
                @"X:",
                spyExternalApplicationRunner,
                fileSystem,
                PaneState.Active,
                view);

            view.HighlightedIndex = 0;

            // Act
            pane.ProcessKey(ConsoleKey.F4.ToKeyInfo());

            // Assert
            spyExternalApplicationRunner.EditedFiles.Should().ContainInOrder(file.FullPath);
        }
    }
}
