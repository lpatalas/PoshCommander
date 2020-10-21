module PoshCommander.Tests.HighlightingCommands

open PoshCommander
open NUnit.Framework
open System.IO
open Swensen.Unquote

let dir (path: string) =
    {
        ItemType = DirectoryItem
        Name = Path.GetFileName(path)
        Path = path
    }

let generateItems count =
    Array.init count (dir << sprintf @"T:\Test\Dir%i")

let createPane rowCount itemCount =
    {
        CurrentDirectory =
            {
                FullPath = @"T:\Test"
                Items = generateItems itemCount
                Name = "Test"
            }
        FirstVisibleIndex = 0
        HighlightedIndex = 0
        IsActive = true
        RowCount = rowCount
    }

let pageSize = 10
let singlePagePane = createPane pageSize pageSize
let twoPagePane = createPane pageSize (pageSize * 2)

module highlightNextItem =
    [<Test>]
    let ``Should increase highlighted index by one``() =
        let initialState = { singlePagePane with HighlightedIndex = 3 }
        let newState = Pane.highlightNextItem initialState
        test <@ newState.HighlightedIndex = initialState.HighlightedIndex + 1 @>

    [<Test>]
    let ``Should do nothing when last item is highlighted``() =
        let initialState =
            { singlePagePane with
                HighlightedIndex = Pane.getLastItemIndex singlePagePane }
        let newState = Pane.highlightNextItem initialState
        test <@ newState = initialState @>

    [<Test>]
    let ``Should increase first visible index when new highlighted item is out of screen``() =
        let initialState =
            { twoPagePane with
                HighlightedIndex = twoPagePane.RowCount
                FirstVisibleIndex = 1 }
        let newState = Pane.highlightNextItem initialState
        test <@ newState.FirstVisibleIndex = initialState.FirstVisibleIndex + 1 @>

module highlightPreviousItem =
    [<Test>]
    let ``Should decrease highlighted index by one``() =
        let initialState = { singlePagePane with HighlightedIndex = 3 }
        let newState = Pane.highlightPreviousItem initialState
        test <@ newState.HighlightedIndex = initialState.HighlightedIndex - 1 @>

    [<Test>]
    let ``Should do nothing when first item is highlighted``() =
        let initialState = { singlePagePane with HighlightedIndex = 0 }
        let newState = Pane.highlightPreviousItem initialState
        test <@ newState = initialState @>

    [<Test>]
    let ``Should decrease first visible index when new highlighted item is not visible``() =
        let initialState =
            { twoPagePane with
                HighlightedIndex = 3
                FirstVisibleIndex = 3 }
        let newState = Pane.highlightPreviousItem initialState
        test <@ newState.FirstVisibleIndex = initialState.FirstVisibleIndex - 1 @>

module highlightFirstItem =
    [<Test>]
    let ``Should set highlighted index to zero``() =
        let initialState = { singlePagePane with HighlightedIndex = 3 }
        let newState = Pane.highlightFirstItem initialState
        test <@ newState.HighlightedIndex = 0 @>

    [<Test>]
    let ``Should do nothing when first item is already highlighted``() =
        let initialState = { singlePagePane with HighlightedIndex = 0 }
        let newState = Pane.highlightFirstItem initialState
        test <@ newState = initialState @>

    [<Test>]
    let ``Should set first visible index to zero if first item is not visible``() =
        let initialState =
            { twoPagePane with
                FirstVisibleIndex = 3
                HighlightedIndex = 3 }
        let newState = Pane.highlightFirstItem initialState
        test <@ newState.FirstVisibleIndex = 0 @>

module highlightLastItem =
    [<Test>]
    let ``Should set highlighted index to index of last item``() =
        let initialState = { singlePagePane with HighlightedIndex = 3 }
        let newState = Pane.highlightLastItem initialState
        test <@ newState.HighlightedIndex = Pane.getLastItemIndex initialState @>

    [<Test>]
    let ``Should do nothing when last item is already highlighted``() =
        let initialState =
            { singlePagePane with
                HighlightedIndex = Pane.getLastItemIndex singlePagePane }
        let newState = Pane.highlightLastItem initialState
        test <@ newState = initialState @>

    [<Test>]
    let ``Should set first visible so that last item is visible on screen``() =
        let initialState =
            { twoPagePane with
                FirstVisibleIndex = 3
                HighlightedIndex = 3 }
        let newState = Pane.highlightLastItem initialState
        test <@ newState.FirstVisibleIndex = Pane.getItemCount newState - newState.RowCount @>

module highlightItemOnePageBefore =
    [<Test>]
    let ``Should set highlighted index to item preceeding current one by row count minus one``() =
        let initialState =
            { twoPagePane with
                FirstVisibleIndex = twoPagePane.RowCount
                HighlightedIndex = twoPagePane.RowCount + 2 }
        let newState = Pane.highlightItemOnePageBefore initialState
        test <@ newState.HighlightedIndex = initialState.HighlightedIndex - initialState.RowCount + 1 @>

    [<Test>]
    let ``Should do nothing when first item is highlighted``() =
        let initialState = { twoPagePane with HighlightedIndex = 0 }
        let newState = Pane.highlightItemOnePageBefore initialState
        test <@ newState = initialState @>

    [<Test>]
    let ``Should set first visible index to index of highlighted item``() =
        let initialState =
            { twoPagePane with
                FirstVisibleIndex = twoPagePane.RowCount
                HighlightedIndex = twoPagePane.RowCount + 2 }
        let newState = Pane.highlightItemOnePageBefore initialState
        test <@ newState.FirstVisibleIndex = newState.HighlightedIndex @>

module highlightItemOnePageAfter =
    [<Test>]
    let ``Should set highlighted index to item succeeding current one by row count minus one``() =
        let initialState = { twoPagePane with HighlightedIndex = 2 }
        let newState = Pane.highlightItemOnePageAfter initialState
        test <@ newState.HighlightedIndex = initialState.HighlightedIndex + initialState.RowCount - 1 @>

    [<Test>]
    let ``Should do nothing when last item is highlighted``() =
        let initialState =
            { twoPagePane with
                FirstVisibleIndex = twoPagePane.RowCount
                HighlightedIndex = Pane.getLastItemIndex twoPagePane }
        let newState = Pane.highlightItemOnePageAfter initialState
        test <@ newState = initialState @>

    [<Test>]
    let ``Should set first visible index so that highlighted item is last visible item``() =
        let initialState =
            { twoPagePane with
                FirstVisibleIndex = 0
                HighlightedIndex = 3 }
        let newState = Pane.highlightItemOnePageAfter initialState
        test <@ newState.FirstVisibleIndex = newState.HighlightedIndex - newState.RowCount + 1 @>
