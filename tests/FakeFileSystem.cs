using System.Collections.Generic;
using System.Linq;

namespace PoshCommander.Tests
{
    public class FakeFileSystem : IFileSystem
    {
        public IList<DirectoryContents> Directories { get; }
            = new List<DirectoryContents>();

        public DirectoryContents GetDirectoryContents(string directoryPath)
            => Directories.Single(d => d.Path == directoryPath);
    }
}
