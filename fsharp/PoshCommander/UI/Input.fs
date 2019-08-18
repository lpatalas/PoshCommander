module PoshCommander.UI.Input

open System

type private InputState
    = PartialInput of string
    | ConfirmedInput of string
    | CancelledInput

let consoleKeySequence =
    Seq.initInfinite (fun _ -> Console.ReadKey(intercept = true))

let readInput (prompt: string) isCharValid keySequence =
    let readNextChar currentInput (keyInfo: ConsoleKeyInfo) =
        let eraseLastChar (input: string) =
            if input.Length > 0 then
                input.Substring(0, input.Length - 1)
            else
                input

        match currentInput with
        | PartialInput inputString ->
            match keyInfo.Key with
            | ConsoleKey.Escape -> CancelledInput
            | ConsoleKey.Enter -> ConfirmedInput inputString
            | ConsoleKey.Backspace -> PartialInput (eraseLastChar inputString)
            | _ when isCharValid keyInfo.KeyChar -> PartialInput (inputString + string keyInfo.KeyChar)
            | _ -> PartialInput inputString
        | _ -> currentInput

    let isFinishedInput input =
        match input with
        | PartialInput _ -> false
        | _ -> true

    let duplicateHead inputSequence =
        seq {
            let head = Seq.head inputSequence
            yield head
            yield head
            yield! inputSequence
        }

    let takeUntil predicate inputSeq =
        inputSeq
        |> duplicateHead
        |> Seq.pairwise
        |> Seq.takeWhile (fst >> predicate >> not)
        |> Seq.map snd

    let input =
        keySequence
        |> Seq.scan readNextChar (PartialInput String.Empty)
        |> takeUntil isFinishedInput
        |> Seq.last

    match input with
    | ConfirmedInput inputString -> Some inputString
    | _ -> None
