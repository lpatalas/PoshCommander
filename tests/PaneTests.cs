using System;
using FluentAssertions;
using Xunit;

namespace PoshCommander.Tests
{
    public class PaneTests
    {
        [Fact]
        public void Should_set_view_PaneState_to_same_value_as_it_was_initialized_with()
        {
            // Arrange
            var paneState = PaneState.Active;
            var paneView = new FakePaneView();

            // Act
            new Pane(@"C:\", paneState, paneView);

            // Assert
            paneView.PaneState.Should().Be(paneState);
        }

        [Fact]
        public void Should_set_pane_title_to_current_directory()
        {
            // Arrange
            var directory = @"C:\Windows\System32";
            var paneView = new FakePaneView();

            // Act
            new Pane(directory, PaneState.Active, paneView);

            // Assert
            paneView.Title.Should().Be(directory);
        }
    }
}
