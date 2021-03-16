module PoshCommander.InputTests

open NUnit.Framework
open PoshCommander
open Swensen.Unquote
open System

let charToConsoleKey c =
    enum (int (Char.ToUpperInvariant(c)))

let charToConsoleKeyInfo c =
    new ConsoleKeyInfo(c, charToConsoleKey c, false, false, false)

let createInputSequence input =
    input |> Seq.map charToConsoleKeyInfo

let readTestInput inputString =
    let pressedKeys = createInputSequence inputString
    Input.readInput (fun _ -> true) ignore pressedKeys

let shouldBeReadAsOption expectedResult inputString =
    let result = readTestInput inputString
    test <@ result = expectedResult @>

let shouldBeReadAs expectedResult =
    shouldBeReadAsOption (Some expectedResult)

let shouldBeReadAsNone =
    shouldBeReadAsOption None

module readInput =
    [<Test>]
    let ``Should return empty string when only Enter is pressed``() =
        "\r" |> shouldBeReadAs ""

    [<Test>]
    let ``Should concatenate all pressed keys until Enter is pressed``() =
        "Input\r" |> shouldBeReadAs "Input"

    [<Test>]
    let ``Should stop reading input immediately after Enter is pressed``() =
        "In\rput" |> shouldBeReadAs "In"

    [<Test>]
    let ``Should erase last character when backspace is pressed``() =
        "In\bpu\bt\r" |> shouldBeReadAs "Ipt"

    [<Test>]
    let ``Should return empty string when all characters are erased``() =
        "Input\b\b\b\b\b\r" |> shouldBeReadAs ""

    [<Test>]
    let ``Should skip all characters for which predicate returns false``() =
        let predicate c =
            c = 'A' || c = 'C' || c = 'E'

        let inputSequence = createInputSequence "ABCDEF\r"
        let result = Input.readInput predicate ignore inputSequence
        test <@ result = Some "ACE" @>

    [<TestCase("\x1b", TestName = "{m}(<ESC>)")>]
    [<TestCase("Inp\x1but", TestName = "{m}(Inp<ESC>ut)")>]
    let ``Should return None when Escape is pressed`` (input: String) =
        input |> shouldBeReadAsNone

    [<TestCase('\r', TestName = "{m}(Enter)")>]
    [<TestCase('\u001b', TestName = "{m}(Escape)")>]
    let ``Should stop reading input after Enter or Escape is pressed`` (breakCharacter: char) =
        let chars = [| 'A'; 'B'; breakCharacter; 'X'; 'Y' |]
        let mutable charIndex = 0

        let inputSequence = seq {
            while charIndex < chars.Length do
                let c = chars.[charIndex]
                charIndex <- charIndex + 1
                yield charToConsoleKeyInfo c
        }

        Input.readInput (fun _ -> true) ignore inputSequence |> ignore

        let nextChar = (Seq.head inputSequence).KeyChar
        test <@ nextChar = 'X' @>

    [<Test>]
    let ``Should execute callback after each typed character``() =
        let mutable inputHistory = []
        let callback input =
            inputHistory <- inputHistory @ [input]

        let inputSequence = createInputSequence "Inp\but\r"
        Input.readInput (fun _ -> true) callback inputSequence |> ignore

        let expectedHistory = [
            ""
            "I"
            "In"
            "Inp"
            "In"
            "Inu"
            "Inut"
        ]

        test <@ inputHistory = expectedHistory @>
