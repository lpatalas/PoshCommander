namespace PoshCommander

open System
open System.Collections.Generic

type ImmutableArray<'TItem when 'TItem : equality> =
    val private items: 'TItem array

    new(items: 'TItem array) =
        {
            items =
                if (isNull items) || (items.Length = 0) then
                    Array.empty
                else
                    items
        }

    member this.Length = this.items.Length
    member this.Items = this.items :> IReadOnlyList<_>

    member private this.IsEqualTo(other: ImmutableArray<'TItem>) =
        Object.ReferenceEquals(this.items, other.items)
        || (this.items = other.items)

    override this.Equals(other: obj) =
        match other with
        | :? ImmutableArray<'TItem> as otherArray -> this.IsEqualTo(otherArray)
        | _ -> false

    override this.GetHashCode() =
        this.items.GetHashCode()

    interface System.Collections.IEnumerable with
        member this.GetEnumerator() =
            (this.items :> System.Collections.IEnumerable).GetEnumerator()

    interface IEnumerable<'TItem> with
        member this.GetEnumerator() =
            this.Items.GetEnumerator()

    interface IEquatable<ImmutableArray<'TItem>> with
        member this.Equals(other: ImmutableArray<'TItem>) =
            this.IsEqualTo(other)

module ImmutableArray =
    let empty<'TItem when 'TItem : equality> : ImmutableArray<'TItem> =
        ImmutableArray(Array.empty)

    let wrap array =
        ImmutableArray(array)

    let fromSeq seq =
        let items = seq |> Seq.toArray
        ImmutableArray(items)

    let isEmpty (array: ImmutableArray<_>) =
        array.Length = 0

    let filter predicate (array: ImmutableArray<_>) =
        array.Items
        |> Seq.filter predicate
        |> fromSeq

    let map mapping (array: ImmutableArray<_>) =
        array.Items
        |> Seq.map mapping
        |> fromSeq

    let skip count (array: ImmutableArray<_>) =
        array.Items
        |> Seq.skip count
        |> fromSeq

    let get index (array: ImmutableArray<_>) =
        array.Items.[index]

    let tryGet index (array: ImmutableArray<_>) =
        if index >= 0 && index < array.Length then
            Some (array.Items.[index])
        else
            None

    let init count initializer =
        Array.init count initializer
        |> ImmutableArray

    let length (array: ImmutableArray<_>) =
        array.Length
