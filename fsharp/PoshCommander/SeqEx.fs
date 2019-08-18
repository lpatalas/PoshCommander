module PoshCommander.SeqEx

let appendBack element seq =
    Seq.append seq (Seq.singleton element)

let takeUntil predicate (inputSeq: 'T seq) = seq {
    use enumerator = inputSeq.GetEnumerator()
    let mutable isDone = false

    while not isDone && enumerator.MoveNext() do
        yield enumerator.Current
        if predicate enumerator.Current then
            isDone <- true
}
