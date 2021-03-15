module PoshCommander.Pane

open System

type Filter =
    | NoFilter
    | Filter of string

type Model =
    {
        CurrentDirectory: Directory
        Filter: Filter
        ListView: ListView.Model<DirectoryItem>
    }

type Msg =
    | ListViewMsg of ListView.Msg<DirectoryItem>
    | KeyPressed of ConsoleKey
    | PageSizeChanged of int
    | ResetFilter
    | SetFilter of string

let init windowHeight path =
    let filterPredicate filter item =
        let name = DirectoryItem.getName item
        name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0

    let directory = Directory.read path
    let items =
        match directory.Content with
        | DirectoryContent items -> items
        | _ -> ImmutableArray.empty

    // window height minus title and status bars
    let pageSize = windowHeight - 2

    {
        CurrentDirectory = directory
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

    let countDirectoriesAndFiles items =
        let dirCount =
            items
            |> Seq.filter DirectoryItem.isDirectory
            |> Seq.length
        let fileCount = ImmutableArray.length items - dirCount
        (dirCount, fileCount)

    let formatStatusText (dirCount, fileCount) =
        let dirLabel = if dirCount = 1 then "directory" else "directories"
        let fileLabel = if fileCount = 1 then "file" else "files"
        sprintf "%i %s / %i %s" dirCount dirLabel fileCount fileLabel

    let text =
        match model.CurrentDirectory.Content with
        | DirectoryContent items ->
            match model.Filter with
            | NoFilter ->
                countDirectoriesAndFiles items
                |> formatStatusText
            | Filter filterString ->
                sprintf "Filter: %s_" filterString
        | DirectoryAccessDenied ->
            "Access Denied"
        | DirectoryNotFound ->
            "Directory Not Found"
        | DirectoryReadError ->
            "Error Reading Directory"

    UI.drawFullLine ui style text

let view uiContext isActive model =
    let (titleArea, rest) =
        UIContext.splitTop uiContext

    let (contentArea, statusArea) =
        UIContext.splitBottom rest

    let currentPath = model.CurrentDirectory.Path
    let itemPresenter = DirectoryItem.getName

    drawTitleBar titleArea isActive currentPath
    ListView.view contentArea itemPresenter model.ListView
    drawStatusBar statusArea model
