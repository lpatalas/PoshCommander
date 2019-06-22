using FluentAssertions;
using PoshCommander.UI;
using Xunit;

namespace PoshCommander.Tests.UI
{
    public class ApplicationTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void When_empty_path_is_specified_for_any_pane_it_should_use_current_location_instead(
            string inputPath)
        {
            // Arrange
            var fileSystem = new StubFileSystem();
            var locationProvider = new StubCurrentLocationProvider();
            var view = new StubApplicationView();

            // Act
            var application = new Application(
                inputPath,
                inputPath,
                new DummyExternalApplicationRunner(),
                fileSystem,
                locationProvider,
                view);

            // Assert
            application.LeftPane.CurrentDirectoryPath.Should().Be(locationProvider.CurrentLocation);
            application.RightPane.CurrentDirectoryPath.Should().Be(locationProvider.CurrentLocation);
        }

        [Fact]
        public void When_initializing_it_should_specify_correct_paths_for_each_pane()
        {
            // Arrange
            var fileSystem = new StubFileSystem();
            var locationProvider = new StubCurrentLocationProvider();
            var view = new StubApplicationView();

            var leftPath = @"X:\Left";
            var rightPath = @"X:\Right";

            // Act
            var application = new Application(
                leftPath,
                rightPath,
                new DummyExternalApplicationRunner(),
                fileSystem,
                locationProvider,
                view);

            // Assert
            var expectedLeftPath = locationProvider.ResolvePath(leftPath);
            var expectedRightPath = locationProvider.ResolvePath(rightPath);

            application.LeftPane.CurrentDirectoryPath.Should().Be(expectedLeftPath);
            application.RightPane.CurrentDirectoryPath.Should().Be(expectedRightPath);
        }
    }
}
