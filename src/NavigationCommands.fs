module PoshCommander.NavigationCommands

let getHighlightedItem pane =
    pane.Items.[pane.HighlightedIndex]

let invokeHighlightedItem invokeDirectory invokeFile pane =
    pane
    |> match getHighlightedItem pane with
        | { ItemType = DirectoryItem } -> invokeDirectory
        | { ItemType = FileItem } -> invokeFile
