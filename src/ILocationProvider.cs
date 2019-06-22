namespace PoshCommander
{
    public interface ILocationProvider
    {
        string CurrentLocation { get; }

        string ResolvePath(string path);
    }
}
