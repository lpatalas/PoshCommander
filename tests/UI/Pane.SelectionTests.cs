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

        [Theory]
        [InlineData(ConsoleKey.UpArrow)]
        [InlineData(ConsoleKey.DownArrow)]
        public void When_ShiftUpArrow_or_ShiftDownArrow_is_pressed_it_should_select_highlighted_item(
            ConsoleKey key)
        {
            // Arrange
            view.HighlightedIndex = 2;
            var highlightedItem = view.GetHighlightedItem();

            // Act
            pane.ProcessKey(key.ToKeyInfo(shift: true));

            // Assert
            view.SelectedItems.Should().BeInStrictOrder(highlightedItem);
        }

        [Theory]
        [InlineData(ConsoleKey.UpArrow)]
        [InlineData(ConsoleKey.DownArrow)]
        public void When_ShiftUpArrow_or_ShiftDownArrow_is_pressed_and_highlighted_item_is_selected_it_should_deselect_it(
            ConsoleKey key)
        {
            // Arrange
            view.HighlightedIndex = 2;
            var highlightedItem = view.GetHighlightedItem();
            view.SelectedItems.Set(highlightedItem);

            // Act
            pane.ProcessKey(key.ToKeyInfo(shift: true));

            // Assert
            view.SelectedItems.Should().BeEmpty();
        }
    }
}
