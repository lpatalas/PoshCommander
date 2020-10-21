module PoshCommander.Tests.NavigationCommands

open NUnit.Framework
open PoshCommander
open Swensen.Unquote
open System.IO

let makeDir (path: string) =
    {
        ItemType = DirectoryItem
        Name = Path.GetFileName(path)
        Path = path
    }

let generateItems directoryCount fileCount targetPath =
    let generateNextItem itemType namePrefix index =
        let name = sprintf "%O%i" namePrefix index
        {
            ItemType = itemType
            Name = name
            Path = Path.Combine(targetPath, name)
        }

    Seq.init directoryCount (generateNextItem DirectoryItem "Dir")
    |> Seq.append
    <| Seq.init fileCount (generateNextItem FileItem "File")
    |> Seq.toArray

let defaultPaneState =
    {
        CurrentDirectory =
            {
                FullPath = @"T:\Test"
                Items = generateItems 3 3 @"T:\Test"
                Name = "Test"
            }
        FirstVisibleIndex = 0
        HighlightedIndex = 0
        IsActive = true
        RowCount = 10
    }

let emptyDirectory fullPath =
    {
        FullPath = fullPath
        Items = Array.empty
        Name = Path.GetFileName(fullPath)
    }

let findIndexOfItemType predicate pane =
    pane.CurrentDirectory.Items
    |> Seq.findIndex predicate

let findIndexByFullPath path items =
    items
    |> Seq.findIndex (fun item -> item.FullPath = path)

let id2 _ x = x

module invokeHighlightedItem =
    [<Test>]
    let ``Should invoke directory callback when highlighted item is a directory``() =
        let directoryIndex = findIndexOfItemType Item.isDirectory defaultPaneState
        let paneState = { defaultPaneState with HighlightedIndex = directoryIndex }
        let mutable invokedDirectoryPath = None
        let directoryCallback path state =
            invokedDirectoryPath <- Some path
            state

        paneState |> Pane.invokeHighlightedItem directoryCallback id2 |> ignore

        test <@ invokedDirectoryPath = Pane.tryGetItemPath directoryIndex paneState @>

    [<Test>]
    let ``Should invoke file callback when highlighted item is a file``() =
        let fileIndex = findIndexOfItemType Item.isFile defaultPaneState
        let paneState = { defaultPaneState with HighlightedIndex = fileIndex }
        let mutable invokedFilePath = None
        let fileCallback path state =
            invokedFilePath <- Some path
            state

        paneState |> Pane.invokeHighlightedItem id2 fileCallback |> ignore
        test <@ invokedFilePath = Pane.tryGetItemPath fileIndex paneState @>

module setCurrentDirectory =
    let emptySubDirectory pane directoryName =
        emptyDirectory (Path.Combine(pane.CurrentDirectory.FullPath, directoryName))

    let fakeDirectoryContent() =
        let path = @"T:\Test"
        {
            (emptyDirectory path) with
                Items = generateItems 3 3 path
        }

    [<Test>]
    let ``Should set current directory to target directory``() =
        let pane = defaultPaneState
        let targetDirectory = fakeDirectoryContent()
        let result = Pane.setCurrentDirectory targetDirectory pane
        test <@ result.CurrentDirectory = targetDirectory @>

    [<Test>]
    let ``Should highlight original directory if new one contains it``() =
        // Arrange
        let originalDirectory = emptyDirectory @"T:\Test\OriginalDir"
        let targetDirectory =
            {
                emptyDirectory @"T:\Test" with
                    Items =
                        [|
                            makeDir @"T:\Test\FirstDir"
                            makeDir originalDirectory.FullPath
                            makeDir @"T:\Test\ThirdDir"
                        |]
            }

        let pane = { defaultPaneState with CurrentDirectory = originalDirectory }
        let result = Pane.setCurrentDirectory targetDirectory pane

        let originalDirectoryIndex =
            result.CurrentDirectory.Items
            |> Seq.findIndex (fun item ->
                match item.ItemType with
                | DirectoryItem -> item.Path = originalDirectory.FullPath
                | _ -> false)

        test <@ result.HighlightedIndex = originalDirectoryIndex @>

    [<Test>]
    let ``Should highlight first item if new directory does not contain previous one``() =
        let originalDirectory = emptyDirectory @"T:\Test\SubDir"
        let targetDirectory = emptyDirectory @"T:\Test\OtherDir"

        let pane = { defaultPaneState with CurrentDirectory = originalDirectory }
        let result = Pane.setCurrentDirectory targetDirectory pane

        test <@ result.HighlightedIndex = 0 @>
