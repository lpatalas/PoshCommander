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

module TestItems =
    type Item = { Index: int; Name: string }

    let init count =
        ImmutableArray.init count (fun i -> { Index = i; Name = sprintf "Item%d" i })

module TestListView =
    let fromItemCount count =
        let items = TestItems.init count
        ListView.init count items

    let fromItems items =
        let itemsArray = items |> ImmutableArray.fromSeq
        ListView.init (ImmutableArray.length itemsArray) itemsArray

    let fromString input =
        let parts = String.split '|' input
        let names =
            parts
            |> Array.map (String.trim "[]<>")
            |> ImmutableArray.wrap
        let highlightedIndex =
            parts
            |> Array.tryFindIndex (fun item -> item.[0] = '>')

        let pageSize = ImmutableArray.length names
        { ListView.init pageSize names with
            HighlightedIndex = highlightedIndex }

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

    let assertModelEquals (initial: ListView.Model<_>) (expected: ListView.Model<_>) (actual: ListView.Model<_>) =
        let formatItems items =
            items
            |> Seq.map string
            |> String.concat "; "
            |> sprintf "[%s]"

        if actual <> expected then
            let initialItems = formatItems initial.Items
            let expectedItems = formatItems expected.Items
            let actualItems = formatItems actual.Items
            let itemsColumnLength = max initialItems.Length (max expectedItems.Length actualItems.Length)

            [
                ""
                sprintf "         | ScrollIndex | HighlightedIndex | PageSize | %s | Selected" (String.padLeft itemsColumnLength "Items")
                sprintf "---------|-------------|------------------|----------|-%s-|----------" (String ('-', itemsColumnLength))
                sprintf "Initial  | %11i | %16s | %8i | %s | %s" initial.FirstVisibleIndex (string initial.HighlightedIndex) initial.PageSize (String.padLeft itemsColumnLength initialItems) (formatItems initial.SelectedItems)
                sprintf "Expected | %11i | %16s | %8i | %s | %s" expected.FirstVisibleIndex (string expected.HighlightedIndex) expected.PageSize (String.padLeft itemsColumnLength expectedItems) (formatItems expected.SelectedItems)
                sprintf "Actual   | %11i | %16s | %8i | %s | %s" actual.FirstVisibleIndex (string actual.HighlightedIndex) actual.PageSize (String.padLeft itemsColumnLength actualItems) (formatItems actual.SelectedItems)
                ""
            ]
            |> String.joinLines
            |> Assert.Fail

    let testMsg msg initialList expectedList =
        let initialModel = ListView.fromAsciiArt id initialList
        let expectedModel = ListView.fromAsciiArt id expectedList

        let actualModel =
            initialModel
            |> ListView.update msg

        assertModelEquals initialModel expectedModel actualModel

    let toTestData input =
        input
        |> Seq.map (fun testCase ->
            let firstArg = testCase |> Seq.map fst |> String.joinLines :> obj
            let secondArg = testCase |> Seq.map snd |> String.joinLines :> obj
            [| firstArg; secondArg |])

    let highlightPreviousItemTestCases =
        [
            [ "> A", "> A"
              "  B", "  B"
              "  C", "  C" ]

            [ "  A", "> A"
              "> B", "  B"
              "  C", "  C" ]

            [ "  A  ", "> A |"
              "> B |", "  B |"
              "  C |", "  C  " ]

            [ "  A  ", "  A  "
              "  B |", "> B |"
              "> C |", "  C |" ]
        ]
        |> toTestData

    [<TestCaseSource(nameof highlightPreviousItemTestCases)>]
    let ``should correctly highlight previous item``(initialList, expectedList) =
        testMsg ListView.HighlightPreviousItem initialList expectedList

    let highlightNextItemTestCases =
        [
            [ "  A", "  A"
              "> B", "  B"
              "  C", "> C" ]

            [ "  A", "  A"
              "  B", "  B"
              "> C", "> C" ]

            [ "> A |", "  A |"
              "  B |", "> B |"
              "  C  ", "  C  " ]

            [ "  A |", "  A  "
              "> B |", "  B |"
              "  C  ", "> C |" ]
        ]
        |> toTestData

    [<TestCaseSource(nameof highlightNextItemTestCases)>]
    let ``should correctly highlight next item``(initialList, expectedList) =
        testMsg ListView.HighlightNextItem initialList expectedList

    let highlightItemOnePageBeforeTestCases =
        [
            [ "> A |", "> A |"
              "  B |", "  B |"
              "  C  ", "  C  " ]

            [ "  A  ", "  A  "
              "  B |", "> B |"
              "  C |", "  C |"
              "> D |", "  D |"
              "  E  ", "  E  " ]

            [ "  A  ", "  A  "
              "  B  ", "  B |"
              "  C |", "> C |"
              "> D |", "  D |"
              "  E |", "  E  " ]

            [ "  A  ", "  A  "
              "  B  ", "> B |"
              "  C  ", "  C |"
              "> D |", "  D |"
              "  E |", "  E  "
              "  F |", "  F  " ]
        ]
        |> toTestData

    [<TestCaseSource(nameof highlightItemOnePageBeforeTestCases)>]
    let ``should correctly highlight item one page before``(initialList, expectedList) =
        testMsg ListView.HighlightItemOnePageBefore initialList expectedList

    let highlightItemOnePageAfterTestCases =
        [
            [ "  A  ", "  A  "
              "  B |", "  B |"
              "> C |", "> C |" ]

            [ "  A  ", "  A  "
              "> B |", "  B |"
              "  C |", "  C |"
              "  D |", "> D |"
              "  E  ", "  E  " ]

            [ "  A |", "  A  "
              "> B |", "  B |"
              "  C |", "  C |"
              "  D  ", "> D |"
              "  E  ", "  E  " ]

            [ "  A |", "  A  "
              "  B |", "  B  "
              "> C |", "  C |"
              "  D  ", "  D |"
              "  E  ", "> E |"
              "  F  ", "  F  " ]
        ]
        |> toTestData

    [<TestCaseSource(nameof highlightItemOnePageAfterTestCases)>]
    let ``should correctly highlight item one page after``(initialList, expectedList) =
        testMsg ListView.HighlightItemOnePageAfter initialList expectedList

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

module SetItemsTests =
    [<Test>]
    let ``should not change state if new items are exactly the same as current ones``() =
        let initialModel = TestListView.fromItemCount 10
        let updatedModel =
            initialModel
            |> ListView.update (ListView.SetItems initialModel.Items)

        test <@ updatedModel = initialModel @>

    [<Test>]
    let ``should update items collection``() =
        let newItems = TestItems.init 12
        let updatedItems =
            TestListView.fromItemCount 10
            |> ListView.update (ListView.SetItems newItems)
            |> ListView.getItems

        test <@ updatedItems = newItems @>

    [<Test>]
    let ``should keep highlight at the same item if it exists in updated collection``() =
        let initialItems = TestItems.init 10
        let highlightedIndex = 3

        let newItems =
            initialItems
            |> ImmutableArray.filter (fun item -> item.Index % highlightedIndex = 0)

        let initialModel =
            { TestListView.fromItems initialItems with HighlightedIndex = Some 3 }
        let initialHighlightedItem =
            initialModel
            |> ListView.getHighlightedItem

        let newHighlightedItem =
            initialModel
            |> ListView.update (ListView.SetItems newItems)
            |> ListView.getHighlightedItem

        test <@ newHighlightedItem = initialHighlightedItem @>


    [<Test>]
    let ``should highlight first item if previously highlighted does not exist in updated collection``() =
        let initialItems = TestItems.init 10
        let highlightedIndex = 3

        let newItems =
            initialItems
            |> ImmutableArray.filter (fun item -> item.Index <> highlightedIndex)

        let newHighlightedIndex =
            { TestListView.fromItems initialItems with
                HighlightedIndex = Some highlightedIndex }
            |> ListView.update (ListView.SetItems newItems)
            |> ListView.getHighlightedIndex

        test <@ newHighlightedIndex = Some 0 @>

    [<Test>]
    let ``should set highlighted index to None if new collection is empty``() =
        let initialItems = TestItems.init 10

        let initialList =
            { TestListView.fromItems initialItems with
                HighlightedIndex = Some 3 }

        let newHighlightedIndex =
            initialList
            |> ListView.update (ListView.SetItems ImmutableArray.empty)
            |> ListView.getHighlightedIndex

        test <@ newHighlightedIndex = None @>

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




