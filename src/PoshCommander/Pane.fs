module PoshCommander.Pane

open System

type PaneItem =
    {
        FullPath: string
        Name: string
    }

type Model =
    {
        CurrentPath: string
        Filter: Filter.Model<PaneItem>
        ListView: ListView.Model<PaneItem>
    }

type Msg =
    | FilterMsg of Filter.Msg
    | ListViewMsg of ListView.Msg<PaneItem>
    | KeyPressed of ConsoleKey
    | PageSizeChanged of int

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
        Filter = Filter.init filterPredicate
        ListView = ListView.init pageSize items
    }

let mapKey (keyInfo: ConsoleKeyInfo) model =
    seq {
        Filter.mapKey keyInfo model.Filter |> Option.map FilterMsg
        ListView.mapKey keyInfo model.ListView |> Option.map ListViewMsg
    }
    |> Seq.tryPick id

let update msg model =
    match msg with
    | FilterMsg filterMsg ->
        { model with Filter = Filter.update filterMsg model.Filter }
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
        match model.Filter.FilterState with
        | Filter.Disabled ->
            "10 Dirs / 18 Files"
        | Filter.Enabled filterString ->
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
