module PoshCommander.App

open System
open System.Management.Automation.Host

type PaneSide =
    | Left
    | Right

let flipPaneSize side =
    match side with
    | Left -> Right
    | Right -> Left

type Model =
    {
        ActivePane: PaneSide
        Host: PSHost
        LeftPane: Pane.Model
        RightPane: Pane.Model
    }

type Msg =
    | ActivePaneMsg of Pane.Msg
    | LayoutChanged of UIContext.Area
    | SwitchActivePane

let private getActivePane model =
    match model.ActivePane with
    | Left -> model.LeftPane
    | Right -> model.RightPane

let setActivePane appModel newPaneModel =
    match appModel.ActivePane with
    | Left -> { appModel with LeftPane = newPaneModel }
    | Right -> { appModel with RightPane = newPaneModel }

let init (host: PSHost) leftPath rightPath =
    let windowHeight = host.UI.RawUI.WindowSize.Height
    let pane = Pane.init Directory.read windowHeight
    {
        ActivePane = Left
        Host = host
        LeftPane = pane leftPath
        RightPane = pane rightPath
    }

let mapKey (keyInfo: ConsoleKeyInfo) model =
    match keyInfo.Key with
    | ConsoleKey.Tab ->
        Some SwitchActivePane
    | _ ->
        Pane.mapKey keyInfo (getActivePane model)
        |> Option.map ActivePaneMsg

let update msg model =
    match msg with
    | ActivePaneMsg paneMsg ->
        getActivePane model
        |> Pane.update paneMsg
        |> setActivePane model
    | LayoutChanged newLayout ->
        model
    | SwitchActivePane ->
        { model with ActivePane = flipPaneSize model.ActivePane }

let view uiContext model =
    let (leftUI, rightUI) = UIContext.splitVertically uiContext
    Pane.view leftUI (model.ActivePane = Left) model.LeftPane
    Pane.view rightUI (model.ActivePane = Right) model.RightPane
