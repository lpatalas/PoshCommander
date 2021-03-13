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
        let items = (ImmutableArray.init count (fun i -> sprintf "Item%d" i))
        ListView.init count items

    let fromItems items =
        let itemsArray = items |> ImmutableArray.fromSeq
        ListView.init (ImmutableArray.length itemsArray) itemsArray

module InitializationTests =
    [<Test>]
    let ``should set highlighted index to None if item collection is empty``() =
        let highlightedIndex =
            ImmutableArray.empty
            |> ListView.init 1
            |> ListView.getHighlightedIndex

        test <@ highlightedIndex = None @>

    [<Test>]
    let ``should set highlighted index to zero if item collection is not empty``() =
        let highlightedIndex =
            ImmutableArray.init 1 (fun i -> "Item")
            |> ListView.init 1
            |> ListView.getHighlightedIndex

        test <@ highlightedIndex = Some 0 @>

module HighlightingTests =
    let initialListView =
        TestListView.fromItemCount 10

    [<TestCase(0, 0)>]
    [<TestCase(1, 0)>]
    [<TestCase(9, 8)>]
    let ``should highlight previous item``(initialIndex, expectedIndex) =
        let updatedIndex =
            { initialListView with
                HighlightedIndex = Some initialIndex }
            |> ListView.update ListView.HighlightPreviousItem
            |> ListView.getHighlightedIndex

        test <@ updatedIndex = Some expectedIndex @>

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
                HighlightedIndex = Some highlightedIndex }
            |> ListView.update ListView.HighlightPreviousItem
            |> ListView.getFirstVisibleIndex

        test <@ updatedIndex = expectedFirstVisibleIndex @>

    [<TestCase(0, 1)>]
    [<TestCase(8, 9)>]
    [<TestCase(9, 9)>]
    let ``should highlight next item``(initialIndex, expectedIndex) =
        let updatedIndex =
            { initialListView with
                HighlightedIndex = Some initialIndex }
            |> ListView.update ListView.HighlightNextItem
            |> ListView.getHighlightedIndex

        test <@ updatedIndex = Some expectedIndex @>

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
                HighlightedIndex = Some highlightedIndex }
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
                HighlightedIndex = Some initialIndex }
            |> ListView.update ListView.HighlightItemOnePageBefore
            |> ListView.getHighlightedIndex

        test <@ updatedIndex = Some expectedIndex @>

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
                HighlightedIndex = Some highlightedIndex }
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
                HighlightedIndex = Some initialIndex }
            |> ListView.update ListView.HighlightItemOnePageAfter
            |> ListView.getHighlightedIndex

        test <@ updatedIndex = Some expectedIndex @>

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
                HighlightedIndex = Some highlightedIndex }
            |> ListView.update ListView.HighlightItemOnePageAfter
            |> ListView.getFirstVisibleIndex

        test <@ updatedIndex = expectedFirstVisibleIndex @>

module SelectionTests =
    let items = Array.init 10 (fun i -> sprintf "Item%d" i)
    let listView = TestListView.fromItems items

    [<Test>]
    let ``should select highlighted item if it was unselected``() =
        let selectedItems =
            { listView with HighlightedIndex = Some 1 }
            |> ListView.update ListView.ToggleItemSelection
            |> ListView.getSelectedItems

        test <@ selectedItems = Set.ofList [ items.[1] ] @>

    [<Test>]
    let ``should unselect highlighted item if it was selected``() =
        let selectedItems =
            { listView with
                HighlightedIndex = Some 1
                SelectedItems = Set.ofList [ items.[1] ] }
            |> ListView.update ListView.ToggleItemSelection
            |> ListView.getSelectedItems

        test <@ selectedItems = Set.empty @>

    [<Test>]
    let ``should revert selection if it was toggled two times``() =
        let selectedItems =
            { listView with HighlightedIndex = Some 1 }
            |> ListView.update ListView.ToggleItemSelection
            |> ListView.update ListView.ToggleItemSelection
            |> ListView.getSelectedItems

        test <@ selectedItems = Set.empty @>

// module FilterTests =
//     let items = [| "abc"; "bbc"; "cab"; "cba" |]
//     let listView = TestListView.fromItems items

//     [<TestCase('a')>]
//     [<TestCase('A')>]
//     [<TestCase('1')>]
//     [<TestCase('_')>]
//     [<TestCase('.')>]
//     [<TestCase(',')>]
//     let ``should start filtering when letter or number is pressed``(c) =
//         let msg =
//             listView
//             |> ListView.mapKey (ConsoleKeyInfo.fromChar c)

//         test <@ msg = Some (ListView.SetFilter (string c)) @>

//     [<TestCase('a')>]
//     [<TestCase('A')>]
//     [<TestCase('1')>]
//     [<TestCase('_')>]
//     [<TestCase('.')>]
//     [<TestCase(',')>]
//     let ``should append character to filter when it is active``(c) =
//         let initialFilter = "abc"
//         let msg =
//             { listView with Filter = ListView.Filter initialFilter }
//             |> ListView.mapKey (ConsoleKeyInfo.fromChar c)

//         let expectedMsg = Some (ListView.SetFilter (initialFilter + string c))
//         test <@ msg = expectedMsg @>

//     [<Test>]
//     let ``should disable filter when escape is pressed``() =
//         let msg =
//             { listView with Filter = ListView.Filter "abc" }
//             |> ListView.mapKey (ConsoleKeyInfo.fromConsoleKey ConsoleKey.Escape)

//         let expectedMsg = Some ListView.ResetFilter
//         test <@ msg = expectedMsg @>

//     [<TestCase("abc", "ab")>]
//     [<TestCase("a", "")>]
//     [<TestCase("", "")>]
//     let ``should erase last character when backspace is pressed``(initialFilter, expectedFilter) =
//         let msg =
//             { listView with Filter = ListView.Filter initialFilter }
//             |> ListView.mapKey (ConsoleKeyInfo.fromConsoleKey ConsoleKey.Backspace)

//         let expectedMsg = Some (ListView.SetFilter expectedFilter)
//         test <@ msg = expectedMsg @>

//     [<Test>]
//     let ``should filter items matching pattern``() =
//         let visibleItems =
//             TestListView.fromItems [ "abc"; "bbc"; "cab"; "cba" ]
//             |> ListView.update (ListView.SetFilter "ab")
//             |> ListView.getVisibleItems

//         let expected = ImmutableArray.wrap [| "abc"; "cab" |]
//         test <@ visibleItems = expected @>

//     [<Test>]
//     let ``should show all items when filter is reset``() =
//         let initialItems = ImmutableArray.wrap [| "abc"; "bbc"; "cab"; "cba" |]
//         let visibleItems =
//             TestListView.fromItems initialItems
//             |> ListView.update (ListView.SetFilter "ab")
//             |> ListView.update ListView.ResetFilter
//             |> ListView.getVisibleItems

//         test <@ visibleItems = initialItems @>

//     [<Test>]
//     let ``should set highlighted index to None when no items are matched by filter``() =
//         let highlightedIndex =
//             [| "abc"; "bbc"; "cab"; "cba" |]
//             |> TestListView.fromItems
//             |> ListView.update (ListView.SetFilter "zzz")
//             |> ListView.getHighlightedIndex

//         test <@ highlightedIndex = None @>

//     [<Test>]
//     let ``should set highlighted index to first visible item when no item was previously highlighted and the new filter matches some items``() =
//         let highlightedIndex =
//             [| "abc"; "bbc"; "cab"; "cba" |]
//             |> TestListView.fromItems
//             |> ListView.update (ListView.SetFilter "zzz")
//             |> ListView.update (ListView.SetFilter "ca")
//             |> ListView.getHighlightedIndex

//         test <@ highlightedIndex = Some 2 @>

//     [<Test>]
//     let ``should set highlighted index to first item when the filter is reset``() =
//         let highlightedIndex =
//             [| "abc"; "bbc"; "cab"; "cba" |]
//             |> TestListView.fromItems
//             |> ListView.update (ListView.SetFilter "zzz")
//             |> ListView.update ListView.ResetFilter
//             |> ListView.getHighlightedIndex

//         test <@ highlightedIndex = Some 0 @>

//     [<Test>]
//     let ``should set highlighted index to None item when the filter is reset but item collection is empty``() =
//         let highlightedIndex =
//             Array.empty
//             |> TestListView.fromItems
//             |> ListView.update (ListView.SetFilter "zzz")
//             |> ListView.update ListView.ResetFilter
//             |> ListView.getHighlightedIndex

//         test <@ highlightedIndex = None @>

//     [<TestCase(1, "bc", 1)>]
//     [<TestCase(2, "bc", 1)>]
//     [<TestCase(3, "bc", 1)>]
//     let ``should move highlight to previous visible items when current one is filtered out``(initialHighlightedIndex, filter, expectedHighlightedIndex) =
//         let initialItems = [| "abc"; "bbc"; "cab"; "cba" |]
//         let highlightedIndex =
//             { TestListView.fromItems initialItems with
//                 HighlightedIndex = Some initialHighlightedIndex }
//             |> ListView.update (ListView.SetFilter filter)
//             |> ListView.getHighlightedIndex

//         test <@ highlightedIndex = Some expectedHighlightedIndex @>

//     [<TestCase([| 'a' |], "a")>]
//     [<TestCase([| 'a'; 'b'; 'c' |], "abc")>]
//     [<TestCase([| 'a'; 'b'; 'c'; '\x08'; 'd' |], "abd")>]
//     [<TestCase([| 'a'; 'b'; '\x08'; '\x08' |], "")>]
//     [<TestCase([| 'a'; '\x08'; '\x08'; '\x08' |], "")>]
//     let ``should set filter to correct value when typing multiple keys``(keys, expectedFilterValue) =
//         let listView = TestListView.fromItems [| "abc"; "bbc"; "cab"; "cba" |]

//         let mapKeyAndUpdate model keyInfo =
//             match ListView.mapKey keyInfo model with
//             | Some msg -> ListView.update msg model
//             | None -> model

//         let updatedFilter =
//             keys
//             |> Seq.map ConsoleKeyInfo.fromChar
//             |> Seq.fold mapKeyAndUpdate listView
//             |> ListView.getFilter

//         let expectedFilter = ListView.Filter expectedFilterValue
//         test <@ updatedFilter = expectedFilter @>




