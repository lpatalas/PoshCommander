module PoshCommander.ListViewTests

open NUnit.Framework
open Swensen.Unquote
open System

module String =
    let contains (substring: string) (input: string) =
        input.IndexOf(substring, StringComparison.Ordinal) >= 0

module ConsoleKeyInfo =
    let fromChar c =
        let shift = Char.IsLetter(c) && Char.IsUpper(c)
        let consoleKey = enum (int (Char.ToUpperInvariant(c)))
        ConsoleKeyInfo(c, consoleKey, shift, alt = false, control = false)

    let fromConsoleKey consoleKey =
        let c = char consoleKey
        ConsoleKeyInfo(c, consoleKey, shift = false, alt = false, control = false)

module TestListView =
    let fromItemCount count =
        let items = (Array.init count (fun i -> sprintf "Item%d" i))
        ListView.init count String.contains items

    let fromItems items =
        let itemsArray = items |> Seq.toArray
        ListView.init (Array.length itemsArray) String.contains itemsArray

module HighlightingTests =
    let initialListView =
        TestListView.fromItemCount 10

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

module SelectionTests =
    let items = Array.init 10 (fun i -> sprintf "Item%d" i)
    let listView = TestListView.fromItems items

    [<Test>]
    let ``should select highlighted item if it was unselected``() =
        let selectedItems =
            { listView with HighlightedIndex = 1 }
            |> ListView.update ListView.ToggleItemSelection
            |> ListView.getSelectedItems

        test <@ selectedItems = Set.ofList [ items.[1] ] @>

    [<Test>]
    let ``should unselect highlighted item if it was selected``() =
        let selectedItems =
            { listView with
                HighlightedIndex = 1
                SelectedItems = Set.ofList [ items.[1] ] }
            |> ListView.update ListView.ToggleItemSelection
            |> ListView.getSelectedItems

        test <@ selectedItems = Set.empty @>

    [<Test>]
    let ``should revert selection if it was toggled two times``() =
        let selectedItems =
            { listView with HighlightedIndex = 1 }
            |> ListView.update ListView.ToggleItemSelection
            |> ListView.update ListView.ToggleItemSelection
            |> ListView.getSelectedItems

        test <@ selectedItems = Set.empty @>

module FilterTests =
    let items = [| "abc"; "bbc"; "cab"; "cba" |]
    let listView = TestListView.fromItems items

    [<TestCase('a')>]
    [<TestCase('A')>]
    [<TestCase('1')>]
    [<TestCase('_')>]
    [<TestCase('.')>]
    [<TestCase(',')>]
    let ``should start filtering when letter or number is pressed``(c) =
        let msg =
            listView
            |> ListView.mapKey (ConsoleKeyInfo.fromChar c)

        test <@ msg = Some (ListView.SetFilter (string c)) @>

    [<TestCase('a')>]
    [<TestCase('A')>]
    [<TestCase('1')>]
    [<TestCase('_')>]
    [<TestCase('.')>]
    [<TestCase(',')>]
    let ``should append character to filter when it is active``(c) =
        let initialFilter = "abc"
        let msg =
            { listView with Filter = ListView.Filter initialFilter }
            |> ListView.mapKey (ConsoleKeyInfo.fromChar c)

        let expectedMsg = Some (ListView.SetFilter (initialFilter + string c))
        test <@ msg = expectedMsg @>

    [<Test>]
    let ``should disable filter when escape is pressed``() =
        let msg =
            { listView with Filter = ListView.Filter "abc" }
            |> ListView.mapKey (ConsoleKeyInfo.fromConsoleKey ConsoleKey.Escape)

        let expectedMsg = Some ListView.ResetFilter
        test <@ msg = expectedMsg @>

    [<TestCase("abc", "ab")>]
    [<TestCase("a", "")>]
    [<TestCase("", "")>]
    let ``should erase last character when backspace is pressed``(initialFilter, expectedFilter) =
        let msg =
            { listView with Filter = ListView.Filter initialFilter }
            |> ListView.mapKey (ConsoleKeyInfo.fromConsoleKey ConsoleKey.Backspace)

        let expectedMsg = Some (ListView.SetFilter expectedFilter)
        test <@ msg = expectedMsg @>

    [<Test>]
    let ``should filter items matching pattern``() =
        let visibleItems =
            TestListView.fromItems [ "abc"; "bbc"; "cab"; "cba" ]
            |> ListView.update (ListView.SetFilter "ab")
            |> ListView.getVisibleItems

        let expected = [| "abc"; "cab" |]
        test <@ visibleItems = expected @>

    [<Test>]
    let ``should show all items when filter is reset``() =
        let initialItems = [| "abc"; "bbc"; "cab"; "cba" |]
        let visibleItems =
            TestListView.fromItems initialItems
            |> ListView.update (ListView.SetFilter "ab")
            |> ListView.update ListView.ResetFilter
            |> ListView.getVisibleItems

        test <@ visibleItems = initialItems @>




