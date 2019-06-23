using System;
using FluentAssertions;
using PoshCommander.Tests.TestDoubles;
using PoshCommander.UI;
using Xunit;

namespace PoshCommander.Tests.UI
{
    public class PaneSelectionTests
    {
        private readonly Pane pane;
        private readonly FakePaneView view;

        public PaneSelectionTests()
        {
            view = new FakePaneView();
            pane = new Pane(
                @"C:\",
                new DummyExternalApplicationRunner(),
                StubFileSystem.FromDirectoryAndFileCount(5, 5),
                PaneState.Active,
                view);

            view.DrawItemsCallCount = 0;
        }

        [Fact]
        public void When_Spacebar_is_pressed_it_should_select_highlighted_item_and_redraw_view()
        {
            // Arrange
            view.HighlightedIndex = 2;

            // Act
            pane.ProcessKey(ConsoleKey.Spacebar.ToKeyInfo());

            // Assert
            view.SelectedItems.Should().BeInStrictOrder(view.GetHighlightedItem());
            view.DrawItemsCallCount.Should().Be(1);
        }

        [Fact]
        public void When_Spacebar_is_pressed_on_already_selected_item_it_should_unselect_it_and_redraw_view()
        {
            // Arrange
            view.HighlightedIndex = 2;
            view.SelectedItems.Set(view.GetHighlightedItem());

            // Act
            pane.ProcessKey(ConsoleKey.Spacebar.ToKeyInfo());

            // Assert
            view.SelectedItems.Should().BeEmpty();
            view.DrawItemsCallCount.Should().Be(1);
        }
    }
}
