module PoshCommander.Tests.InputTests

open FsUnit
open PoshCommander.UI
open System
open NUnit.Framework

let appendBack element seq =
    Seq.append seq (Seq.singleton element)

let replacePlaceholder replacement (input: string) =
    input.Replace("|", replacement)

let charToConsoleKey c =
    enum (int (Char.ToUpperInvariant(c)))

let charToConsoleKeyInfo c =
    new ConsoleKeyInfo(c, charToConsoleKey c, false, false, false)

let createInputSequence input =
    input
    |> Seq.map charToConsoleKeyInfo
    |> appendBack (charToConsoleKeyInfo '\r')

let readTestInput inputString =
    let pressedKeys = createInputSequence inputString
    Input.readInput "Test" (fun _ -> true) pressedKeys

let shouldBeReadAsOption expectedResult inputString =
    let result = readTestInput inputString
    result |> should equal expectedResult

let shouldBeReadAs expectedResult =
    shouldBeReadAsOption (Some expectedResult)

let shouldBeReadAsNone =
    shouldBeReadAsOption None

[<Test>]
let ``Should return empty string when only Enter is pressed``() =
    "" |> shouldBeReadAs ""

[<Test>]
let ``Should concatenate all pressed keys until Enter is pressed``() =
    "Input" |> shouldBeReadAs "Input"

[<Test>]
let ``Should stop reading input immediately after Enter is pressed``() =
    "In\rput" |> shouldBeReadAs "In"

[<Test>]
let ``Should erase last character when backspace is pressed``() =
    "In\bpu\bt" |> shouldBeReadAs "Ipt"

[<Test>]
let ``Should return empty string when all characters are erased``() =
    "Input\b\b\b\b\b" |> shouldBeReadAs ""

[<TestCase("|")>]
[<TestCase("Inp|ut")>]
let ``Should return None when Escape is pressed`` (input: String) =
    input |> replacePlaceholder "\x1b" |> shouldBeReadAsNone

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

    Input.readInput "Test" (fun _ -> true) inputSequence |> ignore

    let head = Seq.head inputSequence
    head.KeyChar |> should equal 'X'
