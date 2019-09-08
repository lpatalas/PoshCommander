module PoshCommander.Tests.NavigationCommands

open NUnit.Framework
open PoshCommander
open PoshCommander.NavigationCommands
open Swensen.Unquote

let defaultPaneState =
    {
        DirectoryPath = @"T:\Test"
        FirstVisibleIndex = 0
        HighlightedIndex = 1
        IsActive = true
        Items = [|
            { FullPath = @"T:\Test\A"; ItemType = DirectoryItem; Name = "A" }
            { FullPath = @"T:\Test\B"; ItemType = FileItem; Name = "B" }
            |]
        RowCount = 10
    }

module invokeHighlightedItem =
    [<Test>]
    let ``should invoke directory continuation when highlighted item is a directory``() =
        let paneState = { defaultPaneState with HighlightedIndex = 0 }
        let directoryCallback state =
            { state with DirectoryPath = "DIRECTORY" }
        let result = paneState |> invokeHighlightedItem directoryCallback id
        test <@ result.DirectoryPath = "DIRECTORY" @>

    [<Test>]
    let ``should invoke file continuation when highlighted item is a file``() =
        let paneState = { defaultPaneState with HighlightedIndex = 1 }
        let fileCallback state =
            { state with DirectoryPath = "FILE" }
        let result = paneState |> invokeHighlightedItem id fileCallback
        test <@ result.DirectoryPath = "FILE" @>

