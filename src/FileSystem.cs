using System;
using System.IO;
using System.Linq;

namespace PoshCommander
{
    public class FileSystem : IFileSystem
    {
        private const string computerDirectoryName = "Computer";

        public DirectoryContents GetDirectoryContents(string directoryPath)
        {
            if (directoryPath.Equals(computerDirectoryName, StringComparison.Ordinal))
                return GetMyComputerDirectoryContents();
            else
                return GetNormalDirectoryContents(directoryPath);
        }

        private DirectoryContents GetNormalDirectoryContents(string directoryPath)
        {

            var directoryInfo = new DirectoryInfo(directoryPath);

            var parentItems = Enumerable.Repeat(
                directoryInfo.Parent != null
                    ? FileSystemItem.CreateParentDirectory(directoryInfo.Parent)
                    : new FileSystemItem(computerDirectoryName, FileSystemItemKind.ParentDirectory, ".."),
                1);

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

        private DirectoryContents GetMyComputerDirectoryContents()
        {
            var drives = DriveInfo.GetDrives();
            var items = drives
                .Select(FileSystemItem.FromDriveInfo)
                .ToList();
            return new DirectoryContents(computerDirectoryName, items);
        }
    }
}
