module PoshCommander.ListViewTests

open NUnit.Framework
open Swensen.Unquote

let dummyListView itemCount =
    ListView.init itemCount (Array.init itemCount (fun i -> sprintf "Item%d" i))

let withHighlightedIndex index (listView: ListView.Model<string>) =
    { listView with HighlightedIndex = index }

let withPageSize pageSize (listView: ListView.Model<string>) =
    { listView with PageSize = pageSize }

let getHighlightedIndex (listView: ListView.Model<string>) =
    listView.HighlightedIndex

module highlightingTests =
    let initialListView =
        dummyListView 10

    [<TestCase(0, 0)>]
    [<TestCase(1, 0)>]
    [<TestCase(9, 8)>]
    let ``should highlight previous item``(initialIndex, expectedIndex) =
        let updatedIndex =
            initialListView
            |> withHighlightedIndex initialIndex
            |> ListView.update ListView.HighlightPreviousItem
            |> getHighlightedIndex

        test <@ updatedIndex = expectedIndex @>

    [<TestCase(0, 1)>]
    [<TestCase(8, 9)>]
    [<TestCase(9, 9)>]
    let ``should highlight next item``(initialIndex, expectedIndex) =
        let updatedIndex =
            initialListView
            |> withHighlightedIndex initialIndex
            |> ListView.update ListView.HighlightNextItem
            |> getHighlightedIndex

        test <@ updatedIndex = expectedIndex @>

    [<TestCase(0, 0)>]
    [<TestCase(1, 0)>]
    [<TestCase(4, 0)>]
    [<TestCase(5, 1)>]
    [<TestCase(9, 5)>]
    let ``should highlight item page before``(initialIndex, expectedIndex) =
        let updatedIndex =
            initialListView
            |> withPageSize 5
            |> withHighlightedIndex initialIndex
            |> ListView.update ListView.HighlightItemOnePageBefore
            |> getHighlightedIndex

        test <@ updatedIndex = expectedIndex @>

    [<TestCase(0, 4)>]
    [<TestCase(5, 9)>]
    [<TestCase(6, 9)>]
    [<TestCase(8, 9)>]
    [<TestCase(9, 9)>]
    let ``should highlight item page after``(initialIndex, expectedIndex) =
        let updatedIndex =
            initialListView
            |> withPageSize 5
            |> withHighlightedIndex initialIndex
            |> ListView.update ListView.HighlightItemOnePageAfter
            |> getHighlightedIndex

        test <@ updatedIndex = expectedIndex @>
