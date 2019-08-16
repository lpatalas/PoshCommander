using System;
using FluentAssertions;
using PoshCommander.Tests.TestDoubles;
using PoshCommander.UI;
using Xunit;

namespace PoshCommander.Tests.UI
{
    public class InputReaderTests
    {
        [Fact]
        public void Should_concatenate_all_keys_read_from_input()
        {
            // Act
            var result = RunReaderOnInput("Test Input");

            // Assert
            result.Should().Be("Test Input");
        }

        [Fact]
        public void Should_ignore_further_input_when_enter_is_pressed()
        {
            // Act
            var result = RunReaderOnInput("test\rinput");

            // Assert
            result.Should().Be("test");
        }

        [Fact]
        public void Should_return_no_result_when_escape_is_pressed()
        {
            // Act
            var result = RunReaderOnInput("Inpu\u001bt");

            // Assert
            result.Should().Be(Option.None);
        }

        [Fact]
        public void Should_erase_previous_character_when_backspace_is_pressed()
        {
            // Act
            var result = RunReaderOnInput("In\bpu\bt");

            // Assert
            result.Should().Be("Ipt");
        }

        [Fact]
        public void Should_ignore_characters_for_which_predicate_returns_false()
        {
            // Act
            var result = RunReaderOnInput(
                "InputInput",
                isCharacterValid: c => c == 'n' || c == 'u');

            // Assert
            result.Should().Be("nunu");
        }

        [Fact]
        public void Should_return_empty_string_when_only_enter_is_pressed()
        {
            // Act
            var result = RunReaderOnInput("\r");

            // Assert
            result.Should().Be(string.Empty);
        }

        [Fact]
        public void Should_return_empty_string_when_all_characters_are_erased()
        {
            // Act
            var result = RunReaderOnInput("Input\b\b\b\b\b");

            // Assert
            result.Should().Be(string.Empty);
        }

        [Fact]
        public void Should_update_status_text_after_each_key_press()
        {
            // Arrange
            var prompt = "Test";
            var view = new FakePaneView
            {
                StatusText = "InitialStatus"
            };

            // Act
            RunReaderOnInput(
                "In\bput",
                prompt: prompt,
                view: view);

            // Assert
            view.StatusTextHistory.Should().BeEquivalentTo(
                "InitialStatus",
                $"{prompt}: ",
                $"{prompt}: I",
                $"{prompt}: In",
                $"{prompt}: I",
                $"{prompt}: Ip",
                $"{prompt}: Ipu",
                $"{prompt}: Iput",
                "InitialStatus");
        }

        private static Option<string> RunReaderOnInput(
            string input,
            Predicate<char> isCharacterValid = null,
            string prompt = null,
            IPaneView view = null)
        {
            var readKey = new StubConsoleReadKeyWrapper(input);
            var inputReader = new InputReader(readKey);

            return inputReader.ReadInput(
                prompt ?? "Test",
                view ?? new FakePaneView(),
                isCharacterValid ?? (_ => true));
        }
    }
}
