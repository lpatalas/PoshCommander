using System.Collections.Generic;

namespace PoshCommander.UI
{
    public interface IPaneView
    {
        int FirstVisibleItemIndex { get; set; }
        int HighlightedIndex { get; set; }
        IReadOnlyList<FileSystemItem> Items { get; set; }
        int MaxVisibleItemCount { get; }
        PaneState PaneState { get; set; }
        string StatusText { get; set; }
        string Title { get; set; }

        void DrawItems();
        void DrawStatusBar();
        void DrawTitleBar();
        void Redraw();
    }
}
