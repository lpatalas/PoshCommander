module PoshCommander.ItemFilter

open System

type Filter =
    | NoFilter
    | Filter of string

type FilterPredicate<'TItem> =
    string -> 'TItem -> bool

type Model<'TItem> =
    {
        Filter: Filter
        FilterPredicate: FilterPredicate<'TItem>
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

let tryMapFilterMsg (keyInfo: ConsoleKeyInfo) model =
    match model.Filter with
    | NoFilter ->
        if isFilterInitChar keyInfo.KeyChar then
            Some (SetFilter (string keyInfo.KeyChar))
        else
            None
    | Filter filterString ->
        if isFilterUpdateChar keyInfo.KeyChar then
            Some (SetFilter (filterString + string keyInfo.KeyChar))
        else if keyInfo.Key = ConsoleKey.Backspace then
            Some (SetFilter (eraseLastChar filterString))
        else if keyInfo.Key = ConsoleKey.Escape then
            Some ResetFilter
        else
            None

