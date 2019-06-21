using System.Collections.Generic;

namespace PoshCommander.Tests
{
    public class FakeFileSystem : IFileSystem
    {
        public IDictionary<string, IReadOnlyList<FileSystemItem>> Directories { get; }
            = new Dictionary<string, IReadOnlyList<FileSystemItem>>();

        public IReadOnlyList<FileSystemItem> GetChildItems(string directoryPath)
            => Directories[directoryPath];
    }
}
