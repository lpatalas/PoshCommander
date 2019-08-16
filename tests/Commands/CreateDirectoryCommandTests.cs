using FluentAssertions;
using PoshCommander.Commands;
using PoshCommander.Tests.TestDoubles;
using Xunit;

namespace PoshCommander.Tests.Commands
{
    public class CreateDirectoryCommandTests
    {
        [Fact]
        public void Should_create_new_directory_in_specified_pane_when_use_provides_valid_input()
        {
            // Arrange
            var directoryName = "newDir";
            var inputReader = new StubInputReader(directoryName);
            var fileSystemPane = new FakeFileSystemPane();
            var command = new CreateDirectoryCommand(inputReader);

            // Act
            command.Execute(fileSystemPane);

            // Assert
            fileSystemPane.CreatedDirectories.Should().BeEquivalentTo(directoryName);
        }

        [Fact]
        public void Should_not_create_new_directory_when_input_is_not_provided_by_user()
        {
            // Arrange
            var inputReader = new StubInputReader(Option.None);
            var fileSystemPane = new FakeFileSystemPane();
            var command = new CreateDirectoryCommand(inputReader);

            // Act
            command.Execute(fileSystemPane);

            // Assert
            fileSystemPane.CreatedDirectories.Should().BeEmpty();
        }
    }
}
