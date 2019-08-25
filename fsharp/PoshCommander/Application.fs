module PoshCommander.Application

open PoshCommander
open System.Management.Automation.Host
open System
open PoshCommander.Commands

let create (windowSize: Size) leftPanePath rightPanePath =
    let middle = windowSize.Width / 2
    let leftPaneBounds = new Rectangle(0, 0, middle - 1, windowSize.Height - 1)
    let rightPaneBounds = new Rectangle(middle + 1, 0, windowSize.Width - 1, windowSize.Height - 1)

    let leftPane = Pane.create leftPaneBounds true leftPanePath 
    let rightPane = Pane.create rightPaneBounds false rightPanePath 

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

let rec run (host: PSHost) applicationState =
    applicationState.LeftPane |> Pane.draw host.UI
    applicationState.RightPane |> Pane.draw host.UI

    let keyInfo = Console.ReadKey(intercept = true)
    let maybeCommand =
        match keyInfo.Key with
        | ConsoleKey.DownArrow -> Some (HighlightingCommands.highlightNextItem |> applyToActivePane)
        | ConsoleKey.End -> Some (HighlightingCommands.highlightLastItem |> applyToActivePane)
        | ConsoleKey.Home -> Some (HighlightingCommands.highlightFirstItem |> applyToActivePane)
        | ConsoleKey.UpArrow -> Some (HighlightingCommands.highlightPreviousItem |> applyToActivePane)
        | ConsoleKey.Escape -> Some ApplicationCommands.quitApplication
        | ConsoleKey.Tab -> Some ApplicationCommands.switchActivePane
        | _ -> None

    let newState =
        match maybeCommand with
        | Some command -> applicationState |> command
        | None -> applicationState

    if newState.IsRunning then
        run host newState
