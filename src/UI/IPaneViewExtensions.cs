using System.Linq;

namespace PoshCommander.UI
{
    public static class IPaneViewExtensions
    {
        public static FileSystemItem GetHighlightedItem(this IPaneView view)
            => view.Items[view.HighlightedIndex];

        public static bool IsItemSelected(this IPaneView view, FileSystemItem item)
            => view.SelectedItems.Contains(item);
    }
}
