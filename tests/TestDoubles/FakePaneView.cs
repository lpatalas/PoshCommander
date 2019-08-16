using System.Collections.Generic;
using System.Management.Automation.Host;
using PoshCommander.UI;

namespace PoshCommander.Tests.TestDoubles
{
    public class FakePaneView : IPaneView
    {
        public Rectangle Bounds { get; set; }
        public int FirstVisibleItemIndex { get; set; }
        public int HighlightedIndex { get; set; }
        public IReadOnlyList<FileSystemItem> Items { get; set; }
        public int MaxVisibleItemCount { get; set; } = 10;
        public PaneState PaneState { get; set; }
        public IList<FileSystemItem> SelectedItems { get; } = new List<FileSystemItem>();

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                _statusTextHistory.Add(value);
            }
        }

        private readonly List<string> _statusTextHistory = new List<string>();
        public IReadOnlyList<string> StatusTextHistory => _statusTextHistory;

        public string Title { get; set; }

        public int DrawItemsCallCount { get; set; }

        public void DrawItems()
        {
            DrawItemsCallCount++;
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
