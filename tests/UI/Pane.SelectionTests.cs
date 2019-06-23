using System;
using System.Linq;
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
            view = new FakePaneView
            {
                MaxVisibleItemCount = 5
            };

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
            view.SelectedItems.Should().BeEquivalentTo(view.GetHighlightedItem());
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
            view.SelectedItems.Should().BeEquivalentTo(highlightedItem);
            view.DrawItemsCallCount.Should().Be(1);
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
            view.DrawItemsCallCount.Should().Be(1);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void When_ShiftDownArrow_is_pressed_on_last_item_it_should_toggle_the_selection_for_it(
            bool wasInitiallySelected)
        {
            // Arrange
            view.HighlightedIndex = view.Items.Count - 1;
            var highlightedItem = view.GetHighlightedItem();

            if (wasInitiallySelected)
                view.SelectedItems.Set(highlightedItem);

            // Act
            pane.ProcessKey(ConsoleKey.DownArrow.ToKeyInfo(shift: true));

            // Assert
            if (wasInitiallySelected)
                view.SelectedItems.Should().BeEmpty();
            else
                view.SelectedItems.Should().BeEquivalentTo(highlightedItem);

            view.DrawItemsCallCount.Should().Be(1);
        }

        [Fact]
        public void When_ShiftPageDown_is_pressed_and_highlighted_item_is_not_selected_it_should_select_all_items_between_old_and_new_highlight_excluding_last_index()
        {
            // Arrange
            view.HighlightedIndex = 0;

            // Act
            pane.ProcessKey(ConsoleKey.PageDown.ToKeyInfo(shift: true));

            // Assert
            var expectedItems = view.Items.Take(view.HighlightedIndex);
            view.SelectedItems.Should().BeEquivalentTo(expectedItems);
            view.DrawItemsCallCount.Should().Be(1);
        }

        [Fact]
        public void When_ShiftPageDown_is_pressed_and_highlighted_item_is_selected_it_should_deselect_all_items_between_old_and_new_highlight_excluding_last_item()
        {
            // Arrange
            view.HighlightedIndex = 1;
            view.MaxVisibleItemCount = 5;
            view.SelectedItems.Set(
                view.Items[0],
                view.Items[1], // <-- highlight before
                view.Items[3],
                view.Items[5], // <-- highlight after
                view.Items[6]);

            // Act
            pane.ProcessKey(ConsoleKey.PageDown.ToKeyInfo(shift: true));

            // Assert
            view.SelectedItems.Should().BeEquivalentTo(
                view.Items[0],
                view.Items[5],
                view.Items[6]);
            view.DrawItemsCallCount.Should().Be(1);
        }

        [Fact]
        public void When_ShiftPageUp_is_pressed_and_highlighted_item_is_not_selected_it_should_select_all_items_between_old_and_new_highlight_excluding_last_index()
        {
            // Arrange
            var originalHighlightIndex = view.MaxVisibleItemCount + 2;
            view.HighlightedIndex = originalHighlightIndex;

            // Act
            pane.ProcessKey(ConsoleKey.PageUp.ToKeyInfo(shift: true));

            // Assert
            var expectedItems = view.Items
                .Skip(view.HighlightedIndex + 1)
                .Take(originalHighlightIndex - view.HighlightedIndex);
            view.SelectedItems.Should().BeEquivalentTo(expectedItems);
            view.DrawItemsCallCount.Should().Be(1);
        }

        [Fact]
        public void When_ShiftPageUp_is_pressed_and_highlighted_item_is_selected_it_should_deselect_all_items_between_old_and_new_highlight_excluding_last_item()
        {
            // Arrange
            view.HighlightedIndex = 6;
            view.MaxVisibleItemCount = 5;
            view.SelectedItems.Set(
                view.Items[1],
                view.Items[2], // <-- highlight after
                view.Items[4],
                view.Items[6], // <-- highlight before
                view.Items[7]); 

            // Act
            pane.ProcessKey(ConsoleKey.PageUp.ToKeyInfo(shift: true));

            // Assert
            view.SelectedItems.Should().BeEquivalentTo(
                view.Items[1],
                view.Items[2],
                view.Items[7]);
            view.DrawItemsCallCount.Should().Be(1);
        }

        [Fact]
        public void When_CtrlA_is_pressed_it_should_select_all_items()
        {
            // Arrange
            view.SelectedItems.Set(view.Items[1], view.Items[3]);

            // Act
            pane.ProcessKey(ConsoleKey.A.ToKeyInfo(control: true));

            // Assert
            view.SelectedItems.Should().BeEquivalentTo(view.Items);
            view.DrawItemsCallCount.Should().Be(1);
        }

        [Fact]
        public void When_CtrlD_is_pressed_it_should_deselect_all_items()
        {
            // Arrange
            view.SelectedItems.Set(view.Items[1], view.Items[3]);

            // Act
            pane.ProcessKey(ConsoleKey.D.ToKeyInfo(control: true));

            // Assert
            view.SelectedItems.Should().BeEmpty();
            view.DrawItemsCallCount.Should().Be(1);
        }

        [Fact]
        public void When_CtrlI_is_pressed_it_should_invert_the_selection()
        {
            // Arrange
            var evenItems = view.Items.Where((_, i) => (i % 2) == 0).ToList();
            var oddItems = view.Items.Where((_, i) => (i % 2) != 0).ToList();

            view.SelectedItems.Set(evenItems);

            // Act
            pane.ProcessKey(ConsoleKey.I.ToKeyInfo(control: true));

            // Assert
            view.SelectedItems.Should().BeEquivalentTo(oddItems);
            view.DrawItemsCallCount.Should().Be(1);
        }
    }
}
