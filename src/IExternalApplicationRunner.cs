namespace PoshCommander
{
    public interface IExternalApplicationRunner
    {
        void RunAssociatedApplication(string filePath);
        void RunEditor(string filePath);
        void RunViewer(string filePath);
    }
}
