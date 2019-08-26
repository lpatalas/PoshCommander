module PoshCommander.Application

open PoshCommander
open System.Management.Automation.Host
open System
open PoshCommander.Commands

let create (windowSize: Size) leftPanePath rightPanePath =
    let middle = windowSize.Width / 2
    let leftPaneBounds = { Left = 0; Top = 0; Width = middle; Height = windowSize.Height }
    let rightPaneBounds = { Left = middle + 1; Top = 0; Width = windowSize.Width - middle - 1; Height = windowSize.Height }
    let paneRowCount = windowSize.Height - 1
    let leftPane = Pane.create paneRowCount true leftPanePath 
    let rightPane = Pane.create paneRowCount false rightPanePath 

    {
        IsRunning = true
        LeftPane = leftPane
        RightPane = rightPane
    }

let applyToActivePane paneCommand application =
    if application.LeftPane.IsActive then
        { application with LeftPane = application.LeftPane |> paneCommand }
    else
        { application with RightPane = application.RightPane |> paneCommand }

let drawPanes (host: PSHost) applicationState =
    let windowSize = host.UI.RawUI.WindowSize
    let middle = windowSize.Width / 2
    let leftPaneBounds = { Left = 0; Top = 0; Width = middle; Height = windowSize.Height }
    let rightPaneBounds = { Left = middle + 1; Top = 0; Width = windowSize.Width - middle - 1; Height = windowSize.Height }
    applicationState.LeftPane |> Pane.draw host.UI leftPaneBounds
    applicationState.RightPane |> Pane.draw host.UI rightPaneBounds

let rec run (host: PSHost) applicationState =
    drawPanes host applicationState

    let keyInfo = Console.ReadKey(intercept = true)
    let maybeCommand =
        match keyInfo.Key with
        | ConsoleKey.DownArrow -> Some (HighlightingCommands.highlightNextItem |> applyToActivePane)
        | ConsoleKey.End -> Some (HighlightingCommands.highlightLastItem |> applyToActivePane)
        | ConsoleKey.Escape -> Some ApplicationCommands.quitApplication
        | ConsoleKey.Home -> Some (HighlightingCommands.highlightFirstItem |> applyToActivePane)
        | ConsoleKey.PageDown -> Some (HighlightingCommands.highlightItemOnePageAfter |> applyToActivePane)
        | ConsoleKey.PageUp -> Some (HighlightingCommands.highlightItemOnePageBefore |> applyToActivePane)
        | ConsoleKey.Tab -> Some ApplicationCommands.switchActivePane
        | ConsoleKey.UpArrow -> Some (HighlightingCommands.highlightPreviousItem |> applyToActivePane)
        | _ -> None

    let newState =
        match maybeCommand with
        | Some command -> applicationState |> command
        | None -> applicationState

    if newState.IsRunning then
        run host newState
