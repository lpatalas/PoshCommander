using System;
using FluentAssertions;
using Xunit;

namespace PoshCommander.Tests
{
    public class PaneInitialStateTests
    {
        [Fact]
        public void When_its_created_it_should_correctly_set_initial_view_state()
        {
            // Arrange
            var directory = @"C:\Windows\System32";
            var paneState = PaneState.Active;
            var view = new FakePaneView();

            var directoryCount = 2;
            var fileCount = 3;
            var fileSystem = StubFileSystem.FromDirectoryAndFileCount(directoryCount, fileCount);

            // Act
            new Pane(directory, fileSystem, paneState, view);

            // Assert
            view.FirstVisibleItemIndex.Should().Be(0);
            view.HighlightedIndex.Should().Be(0);
            view.Items.Should().ContainInOrder(fileSystem.ChildItems);
            view.PaneState.Should().Be(paneState);
            view.StatusText.Should().Be($"Files: {fileCount}, Directories: {directoryCount}");
            view.Title.Should().Be(directory);
        }
    }
}
