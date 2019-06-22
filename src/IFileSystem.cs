namespace PoshCommander
{
    public interface IFileSystem
    {
        DirectoryContents GetDirectoryContents(string directoryPath);
    }
}
