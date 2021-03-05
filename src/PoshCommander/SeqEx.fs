module Seq

let concatWith seq2 seq1 =
    Seq.append seq1 seq2

let takeUntil predicate (inputSeq: 'T seq) = seq {
    use enumerator = inputSeq.GetEnumerator()
    let mutable isDone = false

    while not isDone && enumerator.MoveNext() do
        yield enumerator.Current
        if predicate enumerator.Current then
            isDone <- true
}
