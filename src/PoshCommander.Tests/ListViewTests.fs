module PoshCommander.ListViewTests

open NUnit.Framework
open Swensen.Unquote

module highlightingTests =
    let initialListView =
        ListView.init 10 (Array.init 10 (fun i -> sprintf "Item%d" i))

    [<TestCase(0, 0)>]
    [<TestCase(1, 0)>]
    [<TestCase(9, 8)>]
    let ``should highlight previous item``(initialIndex, expectedIndex) =
        let updatedIndex =
            { initialListView with
                HighlightedIndex = initialIndex }
            |> ListView.update ListView.HighlightPreviousItem
            |> ListView.getHighlightedIndex

        test <@ updatedIndex = expectedIndex @>

    [<TestCase(0, 0, 0)>]
    [<TestCase(1, 0, 0)>]
    [<TestCase(1, 1, 0)>]
    [<TestCase(2, 1, 1)>]
    [<TestCase(4, 1, 1)>]
    let ``should adjust FirstVisibleIndex when highlighting previous item``(highlightedIndex, initialFirstVisibleIndex, expectedFirstVisibleIndex) =
        let updatedIndex =
            { initialListView with
                PageSize = 5
                FirstVisibleIndex = initialFirstVisibleIndex
                HighlightedIndex = highlightedIndex }
            |> ListView.update ListView.HighlightPreviousItem
            |> ListView.getFirstVisibleIndex

        test <@ updatedIndex = expectedFirstVisibleIndex @>

    [<TestCase(0, 1)>]
    [<TestCase(8, 9)>]
    [<TestCase(9, 9)>]
    let ``should highlight next item``(initialIndex, expectedIndex) =
        let updatedIndex =
            { initialListView with
                HighlightedIndex = initialIndex }
            |> ListView.update ListView.HighlightNextItem
            |> ListView.getHighlightedIndex

        test <@ updatedIndex = expectedIndex @>

    [<TestCase(0, 0, 0)>]
    [<TestCase(1, 0, 0)>]
    [<TestCase(3, 0, 0)>]
    [<TestCase(4, 0, 1)>]
    [<TestCase(6, 2, 3)>]
    let ``should adjust FirstVisibleIndex when highlighting next item``(highlightedIndex, initialFirstVisibleIndex, expectedFirstVisibleIndex) =
        let updatedIndex =
            { initialListView with
                PageSize = 5
                FirstVisibleIndex = initialFirstVisibleIndex
                HighlightedIndex = highlightedIndex }
            |> ListView.update ListView.HighlightNextItem
            |> ListView.getFirstVisibleIndex

        test <@ updatedIndex = expectedFirstVisibleIndex @>

    [<TestCase(0, 0)>]
    [<TestCase(1, 0)>]
    [<TestCase(4, 0)>]
    [<TestCase(5, 1)>]
    [<TestCase(9, 5)>]
    let ``should highlight item page before``(initialIndex, expectedIndex) =
        let updatedIndex =
            { initialListView with
                PageSize = 5
                HighlightedIndex = initialIndex }
            |> ListView.update ListView.HighlightItemOnePageBefore
            |> ListView.getHighlightedIndex

        test <@ updatedIndex = expectedIndex @>

    [<TestCase(0, 0, 0)>]
    [<TestCase(1, 0, 0)>]
    [<TestCase(4, 0, 0)>]
    [<TestCase(4, 1, 0)>]
    [<TestCase(5, 5, 1)>]
    [<TestCase(9, 5, 5)>]
    let ``should adjust FirstVisibleIndex when highlighting item one page before``(highlightedIndex, initialFirstVisibleIndex, expectedFirstVisibleIndex) =
        let updatedIndex =
            { initialListView with
                PageSize = 5
                FirstVisibleIndex = initialFirstVisibleIndex
                HighlightedIndex = highlightedIndex }
            |> ListView.update ListView.HighlightItemOnePageBefore
            |> ListView.getFirstVisibleIndex

        test <@ updatedIndex = expectedFirstVisibleIndex @>

    [<TestCase(0, 4)>]
    [<TestCase(5, 9)>]
    [<TestCase(6, 9)>]
    [<TestCase(8, 9)>]
    [<TestCase(9, 9)>]
    let ``should highlight item page after``(initialIndex, expectedIndex) =
        let updatedIndex =
            { initialListView with
                PageSize = 5
                HighlightedIndex = initialIndex }
            |> ListView.update ListView.HighlightItemOnePageAfter
            |> ListView.getHighlightedIndex

        test <@ updatedIndex = expectedIndex @>

    [<TestCase(0, 0, 0)>]
    [<TestCase(1, 0, 1)>]
    [<TestCase(4, 0, 4)>]
    [<TestCase(4, 1, 4)>]
    [<TestCase(8, 4, 5)>]
    [<TestCase(9, 5, 5)>]
    let ``should adjust FirstVisibleIndex when highlighting item one page after``(highlightedIndex, initialFirstVisibleIndex, expectedFirstVisibleIndex) =
        let updatedIndex =
            { initialListView with
                PageSize = 5
                FirstVisibleIndex = initialFirstVisibleIndex
                HighlightedIndex = highlightedIndex }
            |> ListView.update ListView.HighlightItemOnePageAfter
            |> ListView.getFirstVisibleIndex

        test <@ updatedIndex = expectedFirstVisibleIndex @>
