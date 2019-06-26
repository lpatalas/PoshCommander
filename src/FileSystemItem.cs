using System;
using System.IO;

namespace PoshCommander
{
    public class FileSystemItem
    {
        public string FullPath { get; }
        public FileSystemItemKind Kind { get; }
        public string Name { get; }

        public FileSystemItem(
            string fullPath,
            FileSystemItemKind kind,
            string name)
        {
            this.FullPath = fullPath;
            this.Kind = kind;
            this.Name = name;
        }

        public static FileSystemItem FromDriveInfo(DriveInfo drive)
        {
            var driveName = drive.Name.TrimEnd('\\');
            var name = !string.IsNullOrEmpty(drive.VolumeLabel)
                ? $"{driveName} ({drive.VolumeLabel})"
                : driveName;

            return new FileSystemItem(
                drive.RootDirectory.FullName,
                FileSystemItemKind.Directory,
                name);
        }

        public static FileSystemItem FromFileSystemInfo(FileSystemInfo fileSystemInfo)
            => new FileSystemItem(
                fileSystemInfo.FullName,
                GetKindFromInfo(fileSystemInfo),
                fileSystemInfo.Name);

        public static FileSystemItem CreateParentDirectory(FileSystemInfo directoryInfo)
        {
            if (!directoryInfo.Attributes.HasFlag(FileAttributes.Directory))
                throw new ArgumentException("Item must be directory", nameof(directoryInfo));

            return new FileSystemItem(
                directoryInfo.FullName,
                FileSystemItemKind.ParentDirectory,
                "..");
        }

        private static FileSystemItemKind GetKindFromInfo(FileSystemInfo fileSystemInfo)
            => fileSystemInfo.Attributes.HasFlag(FileAttributes.ReparsePoint)
                    ? FileSystemItemKind.SymbolicLink
                : fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory)
                    ? FileSystemItemKind.Directory
                : FileSystemItemKind.File;
    }
}
