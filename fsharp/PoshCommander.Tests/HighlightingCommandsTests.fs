module PoshCommander.Tests.HighlightingCommandsTests

open FsUnit
open PoshCommander
open PoshCommander.Commands.HighlightingCommands
open System.Management.Automation.Host
open NUnit.Framework

let generateItems count =
    Array.init count (fun index ->
        let name = sprintf "Dir%i" index
        {
            FullPath = sprintf "T:\\Test\%s" name
            ItemType = DirectoryItem
            Name = name
        })

let createPane rowCount itemCount =
    {
        DirectoryPath = @"T:\Test"
        FirstVisibleIndex = 0
        HighlightedIndex = 0
        IsActive = true
        Items = generateItems itemCount
        RowCount = rowCount
    }

let singlePagePane = createPane 10 10
let twoPagePane = createPane 10 20

[<Test>]
let ``highlightNextItem command should increase highlighted index by one``() =
    let initialState = { singlePagePane with HighlightedIndex = 3 }
    let newState = highlightNextItem initialState
    newState.HighlightedIndex |> should equal (initialState.HighlightedIndex + 1)

[<Test>]
let ``highlightNextItem command should do nothing when last item is highlighted``() =
    let initialState = { singlePagePane with HighlightedIndex = singlePagePane.Items.Length - 1 }
    let newState = highlightNextItem initialState
    newState |> should equal initialState

[<Test>]
let ``highlightNextItem command increase first visible index when new highlighted item is out of screen``() =
    let initialState =
        { twoPagePane with
            HighlightedIndex = twoPagePane.RowCount
            FirstVisibleIndex = 1 }
    let newState = highlightNextItem initialState
    newState.FirstVisibleIndex |> should equal (initialState.FirstVisibleIndex + 1)

[<Test>]
let ``highlightPreviousItem command should decrease highlighted index by one``() =
    let initialState = { singlePagePane with HighlightedIndex = 3 }
    let newState = highlightPreviousItem initialState
    newState.HighlightedIndex |> should equal (initialState.HighlightedIndex - 1)
    
[<Test>]
let ``highlightPreviousItem command should do nothing when first item is highlighted``() =
    let initialState = { singlePagePane with HighlightedIndex = 0 }
    let newState = highlightPreviousItem initialState
    newState |> should equal initialState

[<Test>]
let ``highlightPreviousItem command should decrease first visible index when new highlighted item is not visible``() =
    let initialState =
        { twoPagePane with
            HighlightedIndex = 3
            FirstVisibleIndex = 3 }
    let newState = highlightPreviousItem initialState
    newState.FirstVisibleIndex |> should equal (initialState.FirstVisibleIndex - 1)
