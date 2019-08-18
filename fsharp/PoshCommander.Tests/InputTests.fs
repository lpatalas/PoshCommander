module PoshCommander.Tests.InputTests

open FsUnit
open PoshCommander.UI
open System
open NUnit.Framework

let appendBack element seq =
    Seq.append seq (Seq.singleton element)

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
let ``Should return None when Escape is pressed``() =
    "In\u001bput" |> shouldBeReadAsNone

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
