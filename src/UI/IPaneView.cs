using System.Collections.Generic;
using System.Management.Automation.Host;

namespace PoshCommander.UI
{
    public interface IPaneView
    {
        Rectangle Bounds { get; }
        int FirstVisibleItemIndex { get; set; }
        int HighlightedIndex { get; set; }
        IReadOnlyList<FileSystemItem> Items { get; set; }
        int MaxVisibleItemCount { get; }
        PaneState PaneState { get; set; }
        IList<FileSystemItem> SelectedItems { get; }
        string StatusText { get; set; }
        string Title { get; set; }

        void DrawItems();
        void DrawStatusBar();
        void DrawTitleBar();
        void Redraw();
    }
}
