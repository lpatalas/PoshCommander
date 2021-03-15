module PoshCommander.ListViewTests

open NUnit.Framework
open System

let assertModelEquals (initial: ListView.Model<_>) (expected: ListView.Model<_>) (actual: ListView.Model<_>) =
    let formatItems items =
        items
        |> Seq.map string
        |> String.concat "; "
        |> sprintf "[%s]"

    let optStr option =
        match option with
        | Some x -> string x
        | None -> "None"

    if actual <> expected then
        let initialItems = formatItems initial.Items
        let expectedItems = formatItems expected.Items
        let actualItems = formatItems actual.Items
        let itemsColumnLength = max initialItems.Length (max expectedItems.Length actualItems.Length)

        [
            ""
            sprintf "         | ScrollIndex | HighlightedIndex | PageSize | %s | Selected" (String.padLeft itemsColumnLength "Items")
            sprintf "---------|-------------|------------------|----------|-%s-|----------" (String ('-', itemsColumnLength))
            sprintf "Initial  | %11i | %16s | %8i | %s | %s" initial.FirstVisibleIndex (optStr initial.HighlightedIndex) initial.PageSize (String.padLeft itemsColumnLength initialItems) (formatItems initial.SelectedItems)
            sprintf "Expected | %11i | %16s | %8i | %s | %s" expected.FirstVisibleIndex (optStr expected.HighlightedIndex) expected.PageSize (String.padLeft itemsColumnLength expectedItems) (formatItems expected.SelectedItems)
            sprintf "Actual   | %11i | %16s | %8i | %s | %s" actual.FirstVisibleIndex (optStr actual.HighlightedIndex) actual.PageSize (String.padLeft itemsColumnLength actualItems) (formatItems actual.SelectedItems)
            ""
        ]
        |> String.joinLines
        |> Assert.Fail

let testAction action initial expected =
    let initialModel = ListView.fromAsciiArt id initial
    let expectedModel = ListView.fromAsciiArt id expected
    let actualModel = action initialModel expectedModel
    assertModelEquals initialModel expectedModel actualModel

let testMsg msg =
    testAction (fun initial _ -> ListView.update msg initial)

let toTestData input =
    input
    |> Seq.map (fun testCase ->
        let firstArg = testCase |> Seq.map fst |> String.joinLines :> obj
        let secondArg = testCase |> Seq.map snd |> String.joinLines :> obj
        [| firstArg; secondArg |])

module InitializationTests =
    [<Test>]
    let ``should set highlighted index to None if item collection is empty``() =
        let actual =
            ImmutableArray.empty
            |> ListView.init 3

        let expected: ListView.Model<_> =
            {
                FirstVisibleIndex = 0
                HighlightedIndex = None
                Items = ImmutableArray.empty
                PageSize = 3
                SelectedItems = Set.empty
            }

        assertModelEquals actual expected actual

    [<Test>]
    let ``should set highlighted index to zero if item collection is not empty``() =
        let items = ImmutableArray.init 4 (sprintf "Item%i")
        let actual = ListView.init 3 items

        let expected: ListView.Model<_> =
            {
                FirstVisibleIndex = 0
                HighlightedIndex = Some 0
                Items = items
                PageSize = 3
                SelectedItems = Set.empty
            }

        assertModelEquals actual expected actual

module HighlightingTests =
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
              "  B  ", "> B |"
              "  C |", "  C |"
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
    let selectionTestCases =
        [
            [ "> A", "> [A]"
              "  B", "   B"
              "  C", "   C" ]

            [ "> [A]", "> A"
              "   B", "   B"
              "   C", "   C" ]

            [ "  [A]", "  [A]"
              ">  B", " > [B]"
              "   C", "    C" ]

            [ "  [A]", "   [A]"
              "> [B]", " >  B "
              "  [C]", "   [C]" ]
        ]
        |> toTestData

    [<TestCaseSource(nameof selectionTestCases)>]
    let ``should correctly toggle selection for highlighted item``(initial, expected) =
        testMsg ListView.ToggleItemSelection initial expected

module SetItemsTests =
    let setItemsTestCases =
        [
            [ "  A  ", "  A "
              "> B |", "> B |"
              "  C |", "  C |" ]

            [ "  A", "  D"
              "> B", "  E"
              "  C", "> B" ]

            [ "  A", "> D"
              "> B", "  E"
              "  C", "  F" ]

            [ "  A", ""
              "> B", ""
              "  C", "" ]

            [ "", "> A"
              "", "  B"
              "", "  C" ]

            [ "> [A]", "> [A]"
              "   B", "    B"
              "   C", "    C" ]

            [ "> [A]", "> D"
              "   B", "   B"
              "   C", "   C" ]

            [ "> [A]", "   D"
              "  [B]", "  [B]"
              "  [C]", "> [A]" ]
        ]
        |> toTestData

    [<TestCaseSource(nameof setItemsTestCases)>]
    let ``should correctly set new items``(initial, expected) =
        let action initial expected =
            initial
            |> ListView.update (ListView.getItems expected |> ListView.SetItems)

        testAction action initial expected

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




