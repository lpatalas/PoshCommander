using System.Collections.Generic;

namespace PoshCommander
{
    public interface IFileSystem
    {
        IReadOnlyList<FileSystemItem> GetChildItems(string directoryPath);
    }
}
