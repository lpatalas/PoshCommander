namespace PoshCommander
{
    public interface IApplicationView
    {
        IPaneView LeftPane { get; }
        IPaneView RightPane { get; }

        void Redraw();
    }
}
