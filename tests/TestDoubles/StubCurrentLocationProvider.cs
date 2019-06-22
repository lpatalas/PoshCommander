namespace PoshCommander.Tests.TestDoubles
{
    public class StubCurrentLocationProvider : ILocationProvider
    {
        public string CurrentLocation { get; set; } = @"X:\Stub";

        public string ResolvePath(string path)
            => $"Resolved:{path}";
    }
}
