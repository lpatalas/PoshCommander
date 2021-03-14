namespace PoshCommander

module Seq =
    let concatWith seq2 seq1 =
        Seq.append seq1 seq2

    let tryMax seq =
        if Seq.isEmpty seq then
            None
        else
            Some (Seq.max seq)

    let tryReduce reducer seq =
        if Seq.isEmpty seq then
            None
        else
            seq
            |> Seq.reduce reducer
            |> Some

    let takeUntil predicate (inputSeq: 'T seq) = seq {
        use enumerator = inputSeq.GetEnumerator()
        let mutable isDone = false

        while not isDone && enumerator.MoveNext() do
            yield enumerator.Current
            if predicate enumerator.Current then
                isDone <- true
    }

    let tryFindItemIndex item seq =
        seq
        |> Seq.tryFindIndex (fun x -> x = item)

    let zipAll (seq1: 'T1 seq) (seq2: 'T2 seq) =
        seq {
            use enumerator1 = seq1.GetEnumerator()
            use enumerator2 = seq2.GetEnumerator()
            let mutable isDone = false

            while not isDone do
                let hasFirst = enumerator1.MoveNext()
                let hasSecond = enumerator2.MoveNext()

                if hasFirst && hasSecond then
                    yield (Some enumerator1.Current, Some enumerator2.Current)
                else if hasFirst then
                    yield (Some enumerator1.Current, None)
                else if hasSecond then
                    yield (None, Some enumerator2.Current)
                else
                    isDone <- true
        }

module String =
    open System

    let endsWith value (input: string) =
        input.EndsWith(value)

    let joinLines input =
        let folder item state =
            sprintf "%s%s%s" item Environment.NewLine state

        input
        |> Seq.tryReduce folder
        |> Option.defaultValue String.Empty

    let padLeft totalLength (input: string) =
        input.PadLeft(totalLength)

    let split (c: char) (input: string) =
        input.Split(c)

    let splitLines (input: string) =
        input.Split([| Environment.NewLine |], StringSplitOptions.None)

    let startsWith value (input: string) =
        input.StartsWith(value)

    let trim (pattern: string) (input: string) =
        let chars = pattern.ToCharArray()
        input.Trim(chars)

    let trimWhitespace =
        trim " \t\n\r"
