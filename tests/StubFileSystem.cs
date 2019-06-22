using System.Collections.Generic;
using System.Linq;

namespace PoshCommander.Tests
{
    public class StubFileSystem : IFileSystem
    {
        public IReadOnlyList<FileSystemItem> ChildItems { get; set; }
            = new List<FileSystemItem>();

        public StubFileSystem()
            : this(Enumerable.Empty<FileSystemItem>())
        {
        }

        public StubFileSystem(IEnumerable<FileSystemItem> childItems)
        {
            this.ChildItems = childItems.ToList();
        }

        public static StubFileSystem FromItemCount(int itemCount)
        {
            var halfOfItemCount = itemCount / 2;
            return FromDirectoryAndFileCount(
                directoryCount: halfOfItemCount,
                fileCount: itemCount - halfOfItemCount);
        }

        public static StubFileSystem FromDirectoryAndFileCount(
            int directoryCount,
            int fileCount)
        {
            var directories = Enumerable.Range(1, directoryCount)
                .Select(n => new FileSystemItem($@"X:\D{n}", FileSystemItemKind.Directory, $"D{n}"));
            var files = Enumerable.Range(1, fileCount)
                .Select(n => new FileSystemItem($@"X:\{n}.txt", FileSystemItemKind.File, "{n}.txt"));

            return new StubFileSystem(directories.Concat(files));
        }

        public DirectoryContents GetDirectoryContents(string directoryPath)
            => new DirectoryContents(directoryPath, ChildItems);
    }
}
