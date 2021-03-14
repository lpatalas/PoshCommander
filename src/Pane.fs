module PoshCommander.Pane

open System

type PaneItem =
    {
        FullPath: string
        Name: string
    }

type Filter =
    | NoFilter
    | Filter of string

type Model =
    {
        CurrentPath: string
        Filter: Filter
        ListView: ListView.Model<PaneItem>
    }

type Msg =
    | ListViewMsg of ListView.Msg<PaneItem>
    | KeyPressed of ConsoleKey
    | PageSizeChanged of int
    | ResetFilter
    | SetFilter of string

let init windowHeight path =
    let makeItem index =
        let name = sprintf "Item%d.txt" index
        {
            FullPath = sprintf "%s\\%s" path name
            Name = name
        }

    let filterPredicate filter item =
        item.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0

    // window height minus title and status bars
    let pageSize = windowHeight - 2

    let items =
        Seq.initInfinite makeItem
        |> Seq.take 10
        |> ImmutableArray.fromSeq

    {
        CurrentPath = path
        Filter = NoFilter
        ListView = ListView.init pageSize items
    }

let mapFilterKey (keyInfo: ConsoleKeyInfo) model =
    let filterInitChars =
        [ 'a'..'z' ]
        @ [ 'A'..'Z' ]
        @ [ '0'..'9' ]
        @ [ ','; '.'; '_' ]
        |> Set.ofList

    let filterUpdateChars =
        filterInitChars
        |> Set.add ' '

    let isFilterInitChar keyChar =
        Set.contains keyChar filterInitChars

    let isFilterUpdateChar keyChar =
        Set.contains keyChar filterUpdateChars

    let eraseLastChar str =
        match str with
        | "" -> ""
        | s -> s.Substring(0, s.Length - 1)

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

let mapKey (keyInfo: ConsoleKeyInfo) model =
    seq {
        mapFilterKey keyInfo model
        ListView.mapKey keyInfo model.ListView |> Option.map ListViewMsg
    }
    |> Seq.tryPick id

let update msg model =
    match msg with
    | ListViewMsg listViewMsg ->
        { model with ListView = ListView.update listViewMsg model.ListView }
    | _ ->
        model

let private drawTitleBar ui isActive location =
    let style =
        if isActive then (Theme.TitleBarActiveForeground, Theme.TitleBarActiveBackground)
        else (Theme.TitleBarInactiveForeground, Theme.TitleBarInactiveBackground)

    UI.initCursor ui
    UI.drawFullLine ui style location

let private drawStatusBar ui model =
    let style = (Theme.StatusBarForeground, Theme.StatusBarBackground)
    UI.initCursor ui

    let text =
        match model.Filter with
        | NoFilter ->
            "10 Dirs / 18 Files"
        | Filter filterString ->
            sprintf "Filter: %s_" filterString

    UI.drawFullLine ui style text

let private itemPresenter item =
    item.Name

let view uiContext isActive model =
    let (titleArea, rest) =
        UIContext.splitTop uiContext

    let (contentArea, statusArea) =
        UIContext.splitBottom rest

    drawTitleBar titleArea isActive model.CurrentPath
    ListView.view contentArea itemPresenter model.ListView
    drawStatusBar statusArea model
