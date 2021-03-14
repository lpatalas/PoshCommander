module PoshCommander.Filter

open System

type FilterState =
    | Disabled
    | Enabled of string

type FilterPredicate<'TItem> =
    string -> 'TItem -> bool

type Model<'TItem> =
    {
        FilterState: FilterState
        FilterPredicate: FilterPredicate<'TItem>
    }

let init predicate =
    {
        FilterState = Disabled
        FilterPredicate = predicate
    }

type Msg =
    | ResetFilter
    | SetFilter of string

let private filterInitChars =
    [ 'a'..'z' ]
    @ [ 'A'..'Z' ]
    @ [ '0'..'9' ]
    @ [ ','; '.'; '_' ]
    |> Set.ofList

let private filterUpdateChars =
    filterInitChars
    |> Set.add ' '

let isFilterInitChar keyChar =
    Set.contains keyChar filterInitChars

let isFilterUpdateChar keyChar =
    Set.contains keyChar filterUpdateChars

let private eraseLastChar str =
    match str with
    | "" -> ""
    | s -> s.Substring(0, s.Length - 1)

let mapKey (keyInfo: ConsoleKeyInfo) model =
    match model.FilterState with
    | Disabled ->
        if isFilterInitChar keyInfo.KeyChar then
            Some (SetFilter (string keyInfo.KeyChar))
        else
            None
    | Enabled filterString ->
        if isFilterUpdateChar keyInfo.KeyChar then
            Some (SetFilter (filterString + string keyInfo.KeyChar))
        else if keyInfo.Key = ConsoleKey.Backspace then
            Some (SetFilter (eraseLastChar filterString))
        else if keyInfo.Key = ConsoleKey.Escape then
            Some ResetFilter
        else
            None

let update msg model =
    match msg with
    | ResetFilter ->
        { model with FilterState = Disabled }
    | SetFilter newFilterString ->
        { model with FilterState = Enabled newFilterString }

let apply items model =
    match model.FilterState with
    | Disabled -> items
    | Enabled filterString ->
        items
        |> ImmutableArray.filter (model.FilterPredicate filterString)

