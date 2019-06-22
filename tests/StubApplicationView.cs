namespace PoshCommander.Tests
{
    public class StubApplicationView : IApplicationView
    {
        public IPaneView LeftPane { get; } = new FakePaneView();
        public IPaneView RightPane { get; } = new FakePaneView();

        public void Redraw()
        {
        }
    }
}
