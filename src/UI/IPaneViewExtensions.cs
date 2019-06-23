using System;

namespace PoshCommander.UI
{
    public static class IPaneViewExtensions
    {
        public static FileSystemItem GetHighlightedItem(this IPaneView view)
            => view.Items[view.HighlightedIndex];

        public static bool IsItemSelected(this IPaneView view, FileSystemItem item)
            => view.SelectedItems.Contains(item);

        public static void SetHighlightedItem(this IPaneView view, FileSystemItem item)
        {
            var itemIndex = view.Items.FirstIndexOf(item);
            if (itemIndex.HasValue)
                view.HighlightedIndex = itemIndex.Value;
            else
                throw new ArgumentException($"Specified item '{item}' is missing from {nameof(view.Items)} collection");
        }
    }
}
