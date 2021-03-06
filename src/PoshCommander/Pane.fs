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
        ListView: ListView.Model<PaneItem>
    }

type Msg =
    | ListViewMsg of ListView.Msg
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
        |> Seq.toArray

    {
        CurrentPath = path
        ListView = ListView.init pageSize filterPredicate items
    }

let mapKey (keyInfo: ConsoleKeyInfo) model =
    ListView.mapKey keyInfo model.ListView
    |> Option.map ListViewMsg

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
        match model.ListView.Filter with
        | ListView.NoFilter -> "10 Dirs / 18 Files"
        | ListView.Filter filterString -> sprintf "Filter: %s" filterString

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
