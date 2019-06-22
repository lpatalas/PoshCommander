using System.Management.Automation;

namespace PoshCommander
{
    public class LocationProvider : ILocationProvider
    {
        private readonly PathIntrinsics pathIntrinsics;

        public string CurrentLocation => pathIntrinsics.CurrentLocation.Path;

        public LocationProvider(PathIntrinsics pathIntrinsics)
        {
            this.pathIntrinsics = pathIntrinsics;
        }

        public string ResolvePath(string path)
            => pathIntrinsics.NormalizeRelativePath(path, string.Empty);
    }
}
