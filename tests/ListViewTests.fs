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
            sprintf "Initial  | %11i | %16s | %8i | %s | %s" initial.ScrollIndex (optStr initial.HighlightedIndex) initial.PageSize (String.padLeft itemsColumnLength initialItems) (formatItems initial.SelectedItems)
            sprintf "Expected | %11i | %16s | %8i | %s | %s" expected.ScrollIndex (optStr expected.HighlightedIndex) expected.PageSize (String.padLeft itemsColumnLength expectedItems) (formatItems expected.SelectedItems)
            sprintf "Actual   | %11i | %16s | %8i | %s | %s" actual.ScrollIndex (optStr actual.HighlightedIndex) actual.PageSize (String.padLeft itemsColumnLength actualItems) (formatItems actual.SelectedItems)
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
                HighlightedIndex = None
                Items = ImmutableArray.empty
                PageSize = 3
                ScrollIndex = 0
                SelectedItems = Set.empty
            }

        assertModelEquals actual expected actual

    [<Test>]
    let ``should set highlighted index to zero if item collection is not empty``() =
        let items = ImmutableArray.init 4 (sprintf "Item%i")
        let actual = ListView.init 3 items

        let expected: ListView.Model<_> =
            {
                HighlightedIndex = Some 0
                Items = items
                PageSize = 3
                ScrollIndex = 0
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
