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

let private itemPresenter item =
    item.Name

let init pageSize path =
    let makeItem index =
        let name = sprintf "Item%d.txt" index
        {
            FullPath = sprintf "%s\\%s" path name
            Name = name
        }

    let items =
        Seq.initInfinite makeItem
        |> Seq.take 100
        |> Seq.toArray

    {
        CurrentPath = path
        ListView = ListView.init (pageSize - 2) itemPresenter items
    }

let mapKey key =
    ListView.mapKey key
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

let private drawStatusBar ui text =
    let style = (Theme.StatusBarForeground, Theme.StatusBarBackground)
    UI.initCursor ui
    UI.drawFullLine ui style text

let view uiContext isActive model =
    let (titleArea, rest) =
        UIContext.splitTop uiContext

    let (contentArea, statusArea) =
        UIContext.splitBottom rest

    drawTitleBar titleArea isActive model.CurrentPath
    ListView.view contentArea model.ListView
    drawStatusBar statusArea "Ready"
