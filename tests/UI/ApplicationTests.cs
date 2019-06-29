using FluentAssertions;
using PoshCommander.Tests.TestDoubles;
using PoshCommander.UI;
using Xunit;

namespace PoshCommander.Tests.UI
{
    public class ApplicationTests
    {
        private readonly StubCurrentLocationProvider locationProvider
            = new StubCurrentLocationProvider();

        private Application CreateApplication(string leftPath, string rightPath)
        {
            return new Application(
                leftPath,
                rightPath,
                new DummyExternalApplicationRunner(),
                new StubFileSystem(),
                locationProvider,
                new StubApplicationView());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void When_empty_path_is_specified_for_any_pane_it_should_use_current_location_instead(
            string inputPath)
        {
            // Arrange
            locationProvider.CurrentLocation = @"T:\Test\Dir";

            // Act
            var application = CreateApplication(inputPath, inputPath);

            // Assert
            application.LeftPane.CurrentDirectoryPath.Should().Be(locationProvider.CurrentLocation);
            application.RightPane.CurrentDirectoryPath.Should().Be(locationProvider.CurrentLocation);
        }

        [Fact]
        public void When_initializing_it_should_specify_correct_paths_for_each_pane()
        {
            // Arrange
            var leftPath = @"X:\Left";
            var rightPath = @"X:\Right";

            // Act
            var application = CreateApplication(leftPath, rightPath);

            // Assert
            var expectedLeftPath = locationProvider.ResolvePath(leftPath);
            var expectedRightPath = locationProvider.ResolvePath(rightPath);

            application.LeftPane.CurrentDirectoryPath.Should().Be(expectedLeftPath);
            application.RightPane.CurrentDirectoryPath.Should().Be(expectedRightPath);
        }
    }
}
