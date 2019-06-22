using System;
using FluentAssertions;
using Xunit;

namespace PoshCommander.Tests
{
    public class PaneFilteringTests
    {
        private static FileSystemItem CreateItem(string name)
            => new FileSystemItem($@"X:\A\{name}", FileSystemItemKind.Directory, name);

        private readonly FileSystemItem abc = CreateItem("abc");
        private readonly FileSystemItem bbb = CreateItem("bbb");
        private readonly FileSystemItem bbc = CreateItem("bbc");
        private readonly FileSystemItem cab = CreateItem("cab");
        private readonly FileSystemItem cba = CreateItem("cba");
        private readonly StubFileSystem fileSystem;
        private readonly Pane pane;
        private readonly FakePaneView view = new FakePaneView();

        public PaneFilteringTests()
        {
            fileSystem = new StubFileSystem(new[] { abc, bbc, cab, cba });
            pane = new Pane(@"X:\A", fileSystem, PaneState.Active, view);
        }

        [Fact]
        public void When_letter_is_pressed_it_should_only_show_items_that_contain_given_letter()
        {
            // Arrange

            // Act
            pane.ProcessKey(ConsoleKey.A.ToKeyInfo());

            // Assert
            pane.Filter.Should().Be("A");
            view.Items.Should().BeInStrictOrder(abc, cab, cba);
        }

        [Fact]
        public void When_second_letter_is_pressed_it_should_append_it_to_previous_one_and_update_filtered_items()
        {
            // Arrange

            // Act
            pane.ProcessKey(ConsoleKey.A.ToKeyInfo());
            pane.ProcessKey(ConsoleKey.B.ToKeyInfo());

            // Assert
            pane.Filter.Should().Be("AB");
            view.Items.Should().BeInStrictOrder(abc, cab);
        }

        [Fact]
        public void When_Backspace_is_pressed_it_should_erase_last_character_from_current_filter()
        {
            // Arrange

            // Act
            pane.ProcessKey(ConsoleKey.A.ToKeyInfo());
            pane.ProcessKey(ConsoleKey.B.ToKeyInfo());
            pane.ProcessKey(ConsoleKey.Backspace.ToKeyInfo());

            // Assert
            pane.Filter.Should().Be("A");
            view.Items.Should().BeInStrictOrder(abc, cab, cba);
        }

        [Fact]
        public void When_Backspace_is_pressed_and_filter_contains_only_one_character_it_should_erase_the_character_but_keep_filter_active()
        {
            // Arrange

            // Act
            pane.ProcessKey(ConsoleKey.A.ToKeyInfo());
            pane.ProcessKey(ConsoleKey.Backspace.ToKeyInfo());

            // Assert
            pane.Filter.Should().Be(string.Empty);
            view.Items.Should().BeInStrictOrder(fileSystem.ChildItems);
        }

        [Fact]
        public void When_letter_is_pressed_but_new_filter_would_not_match_any_item_it_should_be_ignored()
        {
            // Arrange

            // Act
            pane.ProcessKey(ConsoleKey.A.ToKeyInfo());
            pane.ProcessKey(ConsoleKey.Z.ToKeyInfo());

            // Assert
            pane.Filter.Should().Be("A");
            view.Items.Should().BeInStrictOrder(abc, cab, cba);
        }

        [Fact]
        public void When_Escape_is_pressed_it_should_unset_filter()
        {
            // Arrange

            // Act
            pane.ProcessKey(ConsoleKey.A.ToKeyInfo());
            pane.ProcessKey(ConsoleKey.B.ToKeyInfo());
            pane.ProcessKey(ConsoleKey.Escape.ToKeyInfo());

            // Assert
            pane.Filter.Should().Be(Option.None);
            view.Items.Should().BeInStrictOrder(fileSystem.ChildItems);
        }

        [Fact]
        public void When_highlighted_item_is_filtered_out_it_should_move_highlight_to_first_item()
        {
            // Arrange
            view.HighlightedIndex = view.Items.FirstIndexOf(bbc).Value;

            // Act
            pane.ProcessKey(ConsoleKey.A.ToKeyInfo());

            // Assert
            view.HighlightedIndex.Should().Be(0);
        }

        [Fact]
        public void When_highlighted_item_is_matched_by_filter_it_should_keep_highlight_at_the_same_item()
        {
            // Arrange
            view.HighlightedIndex = view.Items.FirstIndexOf(cab).Value;

            // Act
            pane.ProcessKey(ConsoleKey.A.ToKeyInfo());

            // Assert
            var expectedIndex = view.Items.FirstIndexOf(cab).Value;
            view.HighlightedIndex.Should().Be(expectedIndex);
        }
    }
}
