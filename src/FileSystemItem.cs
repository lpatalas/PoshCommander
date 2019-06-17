using System;
using System.IO;

namespace PoshCommander
{
    public class FileSystemItem
    {
        public string FullPath { get; }
        public FileSystemItemKind Kind { get; }
        public string Name { get; }

        public static FileSystemItem FromFileSystemInfo(FileSystemInfo fileSystemInfo)
            => new FileSystemItem(fileSystemInfo, GetKindFromInfo(fileSystemInfo));

        public static FileSystemItem CreateParentDirectory(FileSystemInfo directoryInfo)
        {
            if (!directoryInfo.Attributes.HasFlag(FileAttributes.Directory))
                throw new ArgumentException("Item must be directory", nameof(directoryInfo));

            return new FileSystemItem(
                directoryInfo,
                FileSystemItemKind.ParentDirectory,
                "..");
        }

        private FileSystemItem(
            FileSystemInfo fileSystemInfo,
            FileSystemItemKind kind,
            string nameOverride = null)
        {
            this.FullPath = fileSystemInfo.FullName;
            this.Kind = kind;
            this.Name = nameOverride ?? fileSystemInfo.Name;
        }

        private static FileSystemItemKind GetKindFromInfo(FileSystemInfo fileSystemInfo)
            => fileSystemInfo.Attributes.HasFlag(FileAttributes.ReparsePoint)
                    ? FileSystemItemKind.SymbolicLink
                : fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory)
                    ? FileSystemItemKind.Directory
                : FileSystemItemKind.File;
    }
}
