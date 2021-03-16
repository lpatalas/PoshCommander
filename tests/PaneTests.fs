module PoshCommander.PaneTests

open NUnit.Framework

module FilteringTests =

    let assertEqual expected actual =
        if expected <> actual then
            Assert.Fail(sprintf "Expected: %A; Actual: %A" expected actual)

    let assertItemsEqual expected actual =
        let formatArray items =
            items
            |> ImmutableArray.map DirectoryItem.getName
            |> String.concat ", "

        if expected <> actual then
            let expectedNames = formatArray expected
            let actualNames = formatArray actual

            Assert.Fail(sprintf "Expected: [%s]; Actual: [%s]" expectedNames actualNames)

    let AAA = DirectoryItem.init "AAA" "T:\\AAA" File
    let AAB = DirectoryItem.init "AAB" "T:\\AAB" File
    let ABB = DirectoryItem.init "ABB" "T:\\ABB" File
    let ABC = DirectoryItem.init "ABC" "T:\\ABC" File
    let BCA = DirectoryItem.init "BCA" "T:\\BCA" File

    let initialList =
        [| AAA; AAB; ABB; ABC; BCA |]
        |> ImmutableArray.wrap

    let readDirectory _ =
        {
            Content = DirectoryContent initialList
            Name = "T:\\"
            ParentPath = None
            Path = "T:\\"
        }

    let toTestCaseSource (a, b) =
        [| a :> obj; b :> obj |]

    let showFilteredItemsTestCases =
        [
            (Pane.ResetFilter, initialList)
            (Pane.SetFilter "", initialList)
            (Pane.SetFilter "A", initialList)
            (Pane.SetFilter "AA", [| AAA; AAB |] |> ImmutableArray.wrap)
            (Pane.SetFilter "ABC", [| ABC |] |> ImmutableArray.wrap)
            (Pane.SetFilter "D", ImmutableArray.empty)
            (Pane.SetFilter "AAAA", ImmutableArray.empty)
        ]
        |> Seq.map toTestCaseSource

    [<TestCaseSource(nameof showFilteredItemsTestCases)>]
    let ``should show items matched by filter in list view``(msg, expected) =
        let updatedPane =
            Pane.init readDirectory 10 "T:\\"
            |> Pane.update msg

        assertItemsEqual expected updatedPane.ListView.Items

    let filterKeyTestCases =
        [
            ("", Pane.NoFilter)
            ("\x1b", Pane.NoFilter)
            ("a", Pane.Filter "a")
            ("abc", Pane.Filter "abc")
            ("abc\bd", Pane.Filter "abd")
            ("ab\b\b", Pane.Filter "")
            ("a\b\b\b", Pane.Filter "")
            ("abc\x1b", Pane.NoFilter)
        ]
        |> Seq.map toTestCaseSource

    [<TestCaseSource(nameof filterKeyTestCases)>]
    let ``should set filter to correct value when typing multiple keys``(keys, expectedFilter) =
        let mapKeyAndUpdate model keyInfo =
            match Pane.mapKey keyInfo model with
            | Some msg -> Pane.update msg model
            | None -> model

        let pane = Pane.init readDirectory 10 "T:\\"
        let actualFilter =
            keys
            |> Seq.map ConsoleKeyInfo.fromChar
            |> Seq.fold mapKeyAndUpdate pane
            |> Pane.getFilter

        assertEqual expectedFilter actualFilter
