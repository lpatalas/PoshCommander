module PoshCommander.NavigationCommands

open System.IO

let getHighlightedItem pane =
    pane.Items.[pane.HighlightedIndex]

let invokeHighlightedItem invokeDirectory invokeFile pane =
    let highlightedItem = getHighlightedItem pane
    pane
    |> match highlightedItem with
        | { ItemType = DirectoryItem } -> invokeDirectory highlightedItem
        | { ItemType = FileItem } -> invokeFile highlightedItem

let createDirectoryItem itemType (directoryInfo: FileSystemInfo) =
    {
        FullPath = directoryInfo.FullName
        ItemType = itemType
        Name = directoryInfo.Name
    }

let enumerateDirectory path =
    let directoryInfo = DirectoryInfo(path)
    let directories =
        directoryInfo.EnumerateDirectories()
        |> Seq.map (createDirectoryItem DirectoryItem)
    let files =
        directoryInfo.EnumerateFiles()
        |> Seq.map (createDirectoryItem FileItem)
    directories |> Seq.append files

let navigateToDirectory enumerate targetPath pane =
    let targetItems = enumerate targetPath
    let highlightedIndex =
        let originalDirectoryIndex =
            targetItems
            |> Seq.tryFindIndex (fun item -> item.FullPath = pane.DirectoryPath)
        match originalDirectoryIndex with
        | Some index -> index
        | None -> 0

    { pane with
        DirectoryPath = targetPath
        HighlightedIndex = highlightedIndex
        Items = targetItems }
