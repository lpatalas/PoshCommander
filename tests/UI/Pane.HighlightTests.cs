using System;
using FluentAssertions;
using PoshCommander.Tests.TestDoubles;
using PoshCommander.UI;
using Xunit;

namespace PoshCommander.Tests.UI
{
    public class PaneHighlightTests
    {
        private readonly Pane pane;
        private readonly FakePaneView view;

        public PaneHighlightTests()
        {
            view = new FakePaneView();
            pane = new Pane(
                @"C:\",
                new DummyExternalApplicationRunner(),
                StubFileSystem.FromItemCount(view.MaxVisibleItemCount * 2),
                PaneState.Active,
                view);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_UpArrow_is_pressed_it_should_highlight_previous_item(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = 2;

            // Act
            pane.ProcessKey(ConsoleKey.UpArrow.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.HighlightedIndex.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_UpArrow_is_pressed_and_previous_item_is_not_visible_on_list_it_should_scroll_up_by_one_item(
            bool isShiftPressed)
        {
            // Arrange
            view.FirstVisibleItemIndex = 2;
            view.HighlightedIndex = 2;

            // Act
            pane.ProcessKey(ConsoleKey.UpArrow.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.FirstVisibleItemIndex.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_UpArrow_is_pressed_and_first_item_is_already_highlighted_it_should_leave_highlight_as_is(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = 0;

            // Act
            pane.ProcessKey(ConsoleKey.UpArrow.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.FirstVisibleItemIndex.Should().Be(0);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_Home_is_pressed_it_should_highlight_first_item(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = 3;

            // Act
            pane.ProcessKey(ConsoleKey.Home.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.HighlightedIndex.Should().Be(0);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_Home_is_pressed_it_should_set_first_visible_item_to_first_item(
            bool isShiftPressed)
        {
            // Arrange
            view.FirstVisibleItemIndex = 5;
            view.HighlightedIndex = 5;

            // Act
            pane.ProcessKey(ConsoleKey.Home.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.FirstVisibleItemIndex.Should().Be(0);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_DownArrow_is_pressed_it_should_highlight_next_item(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = 1;

            // Act
            pane.ProcessKey(ConsoleKey.DownArrow.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.HighlightedIndex.Should().Be(2);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_DownArrow_is_pressed_and_next_item_is_not_visible_it_should_scroll_down_by_one_item(
            bool isShiftPressed)
        {
            // Arrange
            view.FirstVisibleItemIndex = 0;
            view.HighlightedIndex = view.MaxVisibleItemCount - 1;

            // Act
            pane.ProcessKey(ConsoleKey.DownArrow.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.FirstVisibleItemIndex.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_DownArrow_is_pressed_and_last_item_is_highlighted_it_should_leave_highlight_as_is(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = view.Items.Count - 1;

            // Act
            pane.ProcessKey(ConsoleKey.DownArrow.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.HighlightedIndex.Should().Be(view.Items.Count - 1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_End_is_pressed_it_should_highlight_last_item(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = 1;

            // Act
            pane.ProcessKey(ConsoleKey.End.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.HighlightedIndex.Should().Be(view.Items.Count - 1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_End_is_pressed_and_last_item_is_not_visible_it_should_scroll_down_to_last_item(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = 1;

            // Act
            pane.ProcessKey(ConsoleKey.End.ToKeyInfo(shift: isShiftPressed));

            // Assert
            var expectedIndex = view.Items.Count - view.MaxVisibleItemCount;
            view.FirstVisibleItemIndex.Should().Be(expectedIndex);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_End_is_pressed_and_last_item_is_already_highlighted_it_should_leave_highlight_as_is(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = view.Items.Count - 1;

            // Act
            pane.ProcessKey(ConsoleKey.End.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.HighlightedIndex.Should().Be(view.Items.Count - 1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_End_is_pressed_and_all_items_are_visible_on_screen_it_should_not_modify_FirstVisibleItemIndex(
            bool isShiftPressed)
        {
            // Arrange
            view.FirstVisibleItemIndex = 0;
            view.HighlightedIndex = 1;
            view.MaxVisibleItemCount = view.Items.Count * 2;

            // Act
            pane.ProcessKey(ConsoleKey.End.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.FirstVisibleItemIndex.Should().Be(0);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_PageUp_is_pressed_it_should_move_highlight_up_by_page_size(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = 6;
            view.MaxVisibleItemCount = 3;

            // Act
            pane.ProcessKey(ConsoleKey.PageUp.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.HighlightedIndex.Should().Be(6 - (view.MaxVisibleItemCount - 1));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_PageUp_is_pressed_and_highlighted_item_index_is_less_than_page_size_it_should_highlight_first_item(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = 3;
            view.MaxVisibleItemCount = 5;

            // Act
            pane.ProcessKey(ConsoleKey.PageUp.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.HighlightedIndex.Should().Be(0);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_PageUp_is_pressed_and_both_current_and_next_highlighted_indices_are_visible_it_should_not_modify_index_of_first_visible_item(
            bool isShiftPressed)
        {
            // Arrange
            view.FirstVisibleItemIndex = 1;
            view.HighlightedIndex = 4;
            view.MaxVisibleItemCount = 3;

            // Act
            pane.ProcessKey(ConsoleKey.PageUp.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.FirstVisibleItemIndex.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_PageUp_is_pressed_and_previous_item_is_not_visible_it_should_scroll_up_so_that_it_becomes_first_visible_item(
            bool isShiftPressed)
        {
            // Arrange
            view.FirstVisibleItemIndex = 5;
            view.HighlightedIndex = 6;
            view.MaxVisibleItemCount = 3;

            // Act
            pane.ProcessKey(ConsoleKey.PageUp.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.FirstVisibleItemIndex.Should().Be(4);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_PageDown_is_pressed_it_should_move_highlight_down_by_page_size(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = 3;
            view.MaxVisibleItemCount = 5;

            // Act
            pane.ProcessKey(ConsoleKey.PageDown.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.HighlightedIndex.Should().Be(3 + (view.MaxVisibleItemCount - 1));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_PageDown_is_pressed_and_highlighted_item_index_is_less_than_page_size_from_end_it_should_highlight_last_item(
            bool isShiftPressed)
        {
            // Arrange
            view.HighlightedIndex = view.Items.Count - 3;
            view.MaxVisibleItemCount = 5;

            // Act
            pane.ProcessKey(ConsoleKey.PageDown.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.HighlightedIndex.Should().Be(view.Items.Count - 1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_PageDown_is_pressed_and_both_current_and_next_highlighted_indices_are_visible_it_should_not_modify_index_of_first_visible_item(
            bool isShiftPressed)
        {
            // Arrange
            view.FirstVisibleItemIndex = 1;
            view.HighlightedIndex = 1;
            view.MaxVisibleItemCount = 3;

            // Act
            pane.ProcessKey(ConsoleKey.PageDown.ToKeyInfo(shift: isShiftPressed));

            // Assert
            view.FirstVisibleItemIndex.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_PageDown_is_pressed_and_newly_highlighted_item_is_not_visible_it_should_scroll_down_so_that_it_becomes_last_visible_item(
            bool isShiftPressed)
        {
            // Arrange
            view.FirstVisibleItemIndex = 1;
            view.HighlightedIndex = 2;
            view.MaxVisibleItemCount = 3;

            // Act
            pane.ProcessKey(ConsoleKey.PageDown.ToKeyInfo(shift: isShiftPressed));

            // Assert
            var expectedHighlightIndex = 2 + view.MaxVisibleItemCount - 1;
            var expectedFirstVisibleIndex = expectedHighlightIndex - (view.MaxVisibleItemCount - 1);
            view.FirstVisibleItemIndex.Should().Be(expectedFirstVisibleIndex);
        }
    }
}
