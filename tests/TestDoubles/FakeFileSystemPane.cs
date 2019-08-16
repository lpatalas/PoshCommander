using System.Collections.Generic;
using PoshCommander.UI;

namespace PoshCommander.Tests.TestDoubles
{
    public class FakeFileSystemPane : IFileSystemPane
    {
        private readonly List<string> _createdDirectories = new List<string>();
        public IReadOnlyList<string> CreatedDirectories => _createdDirectories;

        public IPaneView View { get; } = new FakePaneView();

        public void CreateDirectory(string name)
        {
            _createdDirectories.Add(name);
        }
    }
}
