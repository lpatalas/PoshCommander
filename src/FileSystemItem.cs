using System.IO;

namespace PoshCommander
{
    public class FileSystemItem
    {
        public string FullPath { get; }
        public FileSystemItemKind Kind { get; }
        public string Name { get; }

        public FileSystemItem(FileSystemInfo fileSystemInfo)
            : this(fileSystemInfo, fileSystemInfo.Name)
        {
        }

        public FileSystemItem(FileSystemInfo fileSystemInfo, string name)
        {
            this.FullPath = fileSystemInfo.FullName;
            this.Kind
                = fileSystemInfo.Attributes.HasFlag(FileAttributes.ReparsePoint)
                    ? FileSystemItemKind.SymbolicLink
                : fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory)
                    ? FileSystemItemKind.Directory
                : FileSystemItemKind.File;
            this.Name = name;
        }
    }
}
