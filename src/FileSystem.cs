using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PoshCommander
{
    public class FileSystem : IFileSystem
    {
        public IReadOnlyList<FileSystemItem> GetChildItems(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            var parentItems
                = directoryInfo.Parent != null
                ? Enumerable.Repeat(FileSystemItem.CreateParentDirectory(directoryInfo.Parent), 1)
                : Enumerable.Empty<FileSystemItem>();

            var items = directoryInfo.EnumerateDirectories().Cast<FileSystemInfo>()
                .Concat(directoryInfo.EnumerateFiles().Cast<FileSystemInfo>())
                .Select(FileSystemItem.FromFileSystemInfo);

            return parentItems
                .Concat(items)
                .ToList();
        }
    }
}
