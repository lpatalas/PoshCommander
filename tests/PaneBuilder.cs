namespace PoshCommander.Tests
{
    public static class PaneBuilder
    {
        public static Pane CreateWithView(IPaneView view)
        {
            var fileSystem = StubFileSystem.FromItemCount(view.MaxVisibleItemCount * 2);
            return new Pane(
                @"C:\",
                new DummyExternalApplicationRunner(),
                fileSystem,
                PaneState.Active,
                view);
        }
    }
}
