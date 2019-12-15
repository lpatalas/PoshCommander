module PoshCommander.Tests.NavigationCommands

open NUnit.Framework
open PoshCommander
open PoshCommander.NavigationCommands
open Swensen.Unquote
open System.IO

let generateItems directoryCount fileCount targetPath =
    let generateNextItem itemType index =
        let name = sprintf "%O%i" itemType index
        { FullPath = Path.Combine(targetPath, name); ItemType = itemType; Name = "A" }

    Seq.init directoryCount (generateNextItem DirectoryItem)
    |> Seq.append
    <| Seq.init fileCount (generateNextItem FileItem)
    |> Seq.toArray

let defaultPaneState =
    let path = @"T:\Test"
    {
        DirectoryPath = path
        FirstVisibleIndex = 0
        HighlightedIndex = 1
        IsActive = true
        Items = generateItems 2 3 path
        RowCount = 10
    }

let findIndexOfItemType itemType allItems =
    allItems
    |> Seq.findIndex (fun item -> item.ItemType = itemType)

let findIndexByFullPath path items =
    items
    |> Seq.findIndex (fun item -> item.FullPath = path)

let id2 _ x = x

module invokeHighlightedItem =
    [<Test>]
    let ``Should invoke directory callback when highlighted item is a directory``() =
        let directoryIndex = findIndexOfItemType DirectoryItem defaultPaneState.Items
        let paneState = { defaultPaneState with HighlightedIndex = directoryIndex }
        let mutable invokedDirectory = None
        let directoryCallback item state =
            invokedDirectory <- Some item
            state

        paneState |> invokeHighlightedItem directoryCallback id2 |> ignore
        test <@ invokedDirectory = Some paneState.Items.[directoryIndex] @>

    [<Test>]
    let ``Should invoke file callback when highlighted item is a file``() =
        let fileIndex = findIndexOfItemType FileItem defaultPaneState.Items
        let paneState = { defaultPaneState with HighlightedIndex = fileIndex }
        let mutable invokedFile = None
        let fileCallback item state =
            invokedFile <- Some item
            state

        paneState |> invokeHighlightedItem id2 fileCallback |> ignore
        test <@ invokedFile = Some paneState.Items.[fileIndex] @>

module navigateToDirectory =
    [<Test>]
    let ``Should set directory path to path of target directory``() =
        let paneState = defaultPaneState
        let targetPath = Path.Combine(paneState.DirectoryPath, "SubDir")
        let enumerate _ = Array.empty
        let result = navigateToDirectory enumerate targetPath paneState
        test <@ result.DirectoryPath = targetPath @>

    [<Test>]
    let ``Should fill pane items with contents of target directory``() =
        let paneState = defaultPaneState
        let targetPath = Path.Combine(paneState.DirectoryPath, "SubDir")
        let enumerate = generateItems 2 2
        let expectedItems = enumerate targetPath
        let result = paneState |> navigateToDirectory enumerate targetPath
        test <@ result.Items = expectedItems @>

    [<Test>]
    let ``Should highlight original directory if new one contains it``() =
        let targetPath = @"T:\Test"
        let originalPath = Path.Combine(targetPath, "SubDir")
        let paneState = { defaultPaneState with DirectoryPath = originalPath }
        let enumerate path =
            generateItems 2 0 path
            |> Array.append ([| { FullPath = originalPath; ItemType = DirectoryItem; Name = Path.GetFileName(originalPath) } |])
            |> Array.append (generateItems 2 0 path)

        let result = paneState |> navigateToDirectory enumerate targetPath
        let originalDirectoryIndex = result.Items |> findIndexByFullPath originalPath
        test <@ result.HighlightedIndex = originalDirectoryIndex @>
        
    [<Test>]
    let ``Should highlight first item if new directory does not contain original one``() =
        let originalPath = @"T:\Test\Original"
        let targetPath = @"T:\Test\Target"
        let paneState = { defaultPaneState with DirectoryPath = originalPath }
        let enumerate = generateItems 3 0
        let result = paneState |> navigateToDirectory enumerate targetPath
        test <@ result.HighlightedIndex = 0 @>
