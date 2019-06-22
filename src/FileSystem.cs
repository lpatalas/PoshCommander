using System;
using System.IO;
using System.Linq;

namespace PoshCommander
{
    public class FileSystem : IFileSystem
    {
        public DirectoryContents GetDirectoryContents(string directoryPath)
        {
            var directoryInfo = new DirectoryInfo(directoryPath);

            var parentItems
                = directoryInfo.Parent != null
                ? Enumerable.Repeat(FileSystemItem.CreateParentDirectory(directoryInfo.Parent), 1)
                : Enumerable.Empty<FileSystemItem>();

            try
            {
                var childItems = directoryInfo.EnumerateDirectories().Cast<FileSystemInfo>()
                    .Concat(directoryInfo.EnumerateFiles().Cast<FileSystemInfo>())
                    .Select(FileSystemItem.FromFileSystemInfo);

                var items = parentItems
                    .Concat(childItems)
                    .ToList();

                return new DirectoryContents(directoryPath, items);
            }
            catch (UnauthorizedAccessException)
            {
                return DirectoryContents.AccessNotAllowed(
                    directoryPath,
                    parentItems.ToList());
            }
        }
    }
}
