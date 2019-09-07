module PoshCommander.Tests.HighlightingCommandsTests

open PoshCommander
open PoshCommander.Commands.HighlightingCommands
open NUnit.Framework
open Swensen.Unquote

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

let pageSize = 10
let singlePagePane = createPane pageSize pageSize
let twoPagePane = createPane pageSize (pageSize * 2)

[<Test>]
let ``highlightNextItem command should increase highlighted index by one``() =
    let initialState = { singlePagePane with HighlightedIndex = 3 }
    let newState = highlightNextItem initialState
    test <@ newState.HighlightedIndex = initialState.HighlightedIndex + 1 @>

[<Test>]
let ``highlightNextItem command should do nothing when last item is highlighted``() =
    let initialState = { singlePagePane with HighlightedIndex = singlePagePane.Items.Length - 1 }
    let newState = highlightNextItem initialState
    test <@ newState = initialState @>

[<Test>]
let ``highlightNextItem command increase first visible index when new highlighted item is out of screen``() =
    let initialState =
        { twoPagePane with
            HighlightedIndex = twoPagePane.RowCount
            FirstVisibleIndex = 1 }
    let newState = highlightNextItem initialState
    test <@ newState.FirstVisibleIndex = initialState.FirstVisibleIndex + 1 @>

[<Test>]
let ``highlightPreviousItem command should decrease highlighted index by one``() =
    let initialState = { singlePagePane with HighlightedIndex = 3 }
    let newState = highlightPreviousItem initialState
    test <@ newState.HighlightedIndex = initialState.HighlightedIndex - 1 @>
    
[<Test>]
let ``highlightPreviousItem command should do nothing when first item is highlighted``() =
    let initialState = { singlePagePane with HighlightedIndex = 0 }
    let newState = highlightPreviousItem initialState
    test <@ newState = initialState @>

[<Test>]
let ``highlightPreviousItem command should decrease first visible index when new highlighted item is not visible``() =
    let initialState =
        { twoPagePane with
            HighlightedIndex = 3
            FirstVisibleIndex = 3 }
    let newState = highlightPreviousItem initialState
    test <@ newState.FirstVisibleIndex = initialState.FirstVisibleIndex - 1 @>

[<Test>]
let ``highlightFirstItem should set highlighted index to zero``() =
    let initialState = { singlePagePane with HighlightedIndex = 3 }
    let newState = highlightFirstItem initialState
    test <@ newState.HighlightedIndex = 0 @>

[<Test>]
let ``highlightFirstItem should do nothing when first item is already highlighted``() =
    let initialState = { singlePagePane with HighlightedIndex = 0 }
    let newState = highlightFirstItem initialState
    test <@ newState = initialState @>

[<Test>]
let ``highlightFirstItem should set first visible index to zero if first item is not visible``() =
    let initialState =
        { twoPagePane with
            FirstVisibleIndex = 3
            HighlightedIndex = 3 }
    let newState = highlightFirstItem initialState
    test <@ newState.FirstVisibleIndex = 0 @>

[<Test>]
let ``highlightLastItem should set highlighted index to index of last item``() =
    let initialState = { singlePagePane with HighlightedIndex = 3 }
    let newState = highlightLastItem initialState
    test <@ newState.HighlightedIndex = initialState.Items.Length - 1 @>

[<Test>]
let ``highlightLastItem should do nothing when last item is already highlighted``() =
    let initialState =
        { singlePagePane with
            HighlightedIndex = singlePagePane.Items.Length - 1 }
    let newState = highlightLastItem initialState
    test <@ newState = initialState @>

[<Test>]
let ``highlightLastItem should set first visible so that last item is visible on screen``() =
    let initialState =
        { twoPagePane with
            FirstVisibleIndex = 3
            HighlightedIndex = 3 }
    let newState = highlightLastItem initialState
    test <@ newState.FirstVisibleIndex = newState.Items.Length - newState.RowCount @>

[<Test>]
let ``highlightItemOnePageBefore should set highlighted index to item preceeding current one by row count minus one``() =
    let initialState =
        { twoPagePane with
            FirstVisibleIndex = twoPagePane.RowCount
            HighlightedIndex = twoPagePane.RowCount + 2 }
    let newState = highlightItemOnePageBefore initialState
    test <@ newState.HighlightedIndex = initialState.HighlightedIndex - initialState.RowCount + 1 @>

[<Test>]
let ``highlightItemOnePageBefore should do nothing when first item is highlighted``() =
    let initialState = { twoPagePane with HighlightedIndex = 0 }
    let newState = highlightItemOnePageBefore initialState
    test <@ newState = initialState @>

[<Test>]
let ``highlightItemOnePageBefore should set first visible index to index of highlighted item``() =
    let initialState =
        { twoPagePane with
            FirstVisibleIndex = twoPagePane.RowCount
            HighlightedIndex = twoPagePane.RowCount + 2 }
    let newState = highlightItemOnePageBefore initialState
    test <@ newState.FirstVisibleIndex = newState.HighlightedIndex @>

[<Test>]
let ``highlightItemOnePageAfter should set highlighted index to item succeeding current one by row count minus one``() =
    let initialState = { twoPagePane with HighlightedIndex = 2 }
    let newState = highlightItemOnePageAfter initialState
    test <@ newState.HighlightedIndex = initialState.HighlightedIndex + initialState.RowCount - 1 @>
    
[<Test>]
let ``highlightItemOnePageAfter should do nothing when last item is highlighted``() =
    let initialState =
        { twoPagePane with
            FirstVisibleIndex = twoPagePane.RowCount
            HighlightedIndex = twoPagePane.Items.Length - 1 }
    let newState = highlightItemOnePageAfter initialState
    test <@ newState = initialState @>

[<Test>]
let ``highlightItemOnePageAfter should set first visible index so that highlighted item is last visible item``() =
    let initialState =
        { twoPagePane with
            FirstVisibleIndex = 0
            HighlightedIndex = 3 }
    let newState = highlightItemOnePageAfter initialState
    test <@ newState.FirstVisibleIndex = newState.HighlightedIndex - newState.RowCount + 1 @>
