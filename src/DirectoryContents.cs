using System.Collections.Generic;

namespace PoshCommander
{
    public class DirectoryContents
    {
        public bool IsAccessAllowed { get; }
        public IReadOnlyList<FileSystemItem> Items { get; }
        public string Path { get; }

        public DirectoryContents(
            string path,
            IReadOnlyList<FileSystemItem> items)
            : this(true, items, path)
        {
        }

        public static DirectoryContents AccessNotAllowed(
            string path,
            IReadOnlyList<FileSystemItem> items)
            => new DirectoryContents(false, items, path);

        private DirectoryContents(
            bool isAccessAllowed,
            IReadOnlyList<FileSystemItem> items,
            string path)
        {
            this.IsAccessAllowed = isAccessAllowed;
            this.Items = items;
            this.Path = path;
        }
    }
}
