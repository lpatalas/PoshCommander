using System.Collections.Generic;
using PoshCommander.UI;

namespace PoshCommander.Tests.TestDoubles
{
    public class FakePaneView : IPaneView
    {
        public int FirstVisibleItemIndex { get; set; }
        public int HighlightedIndex { get; set; }
        public IReadOnlyList<FileSystemItem> Items { get; set; }

        public int MaxVisibleItemCount { get; set; } = 10;

        public PaneState PaneState { get; set; }
        public string StatusText { get; set; }
        public string Title { get; set; }

        public void DrawItems()
        {
        }

        public void DrawStatusBar()
        {
        }

        public void DrawTitleBar()
        {
        }

        public void Redraw()
        {
        }
    }
}
