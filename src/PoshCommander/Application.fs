namespace PoshCommander

open PoshCommander
open System.Management.Automation.Host
open System

type Application = {
    IsRunning: bool
    LeftPane: Pane
    RightPane: Pane
    }

module Application =
    let create (windowSize: Size) leftPanePath rightPanePath =
        let paneRowCount = windowSize.Height - 1
        let leftPane = Pane.create paneRowCount true leftPanePath
        let rightPane = Pane.create paneRowCount false rightPanePath

        {
            IsRunning = true
            LeftPane = leftPane
            RightPane = rightPane
        }

    let drawPanes (host: PSHost) applicationState =
        let windowSize = host.UI.RawUI.WindowSize
        let middle = windowSize.Width / 2
        let leftPaneBounds = { Left = 0; Top = 0; Width = middle; Height = windowSize.Height }
        let rightPaneBounds = { Left = middle + 1; Top = 0; Width = windowSize.Width - middle - 2; Height = windowSize.Height }
        applicationState.LeftPane |> Pane.draw host.UI leftPaneBounds
        applicationState.RightPane |> Pane.draw host.UI rightPaneBounds

    let rec run (host: PSHost) mapCommand applicationState =
        drawPanes host applicationState

        let keyInfo = Console.ReadKey(intercept = true)
        let maybeCommand = mapCommand keyInfo

        let newState =
            match maybeCommand with
            | Some command -> applicationState |> command
            | None -> applicationState

        if newState.IsRunning then
            run host mapCommand newState
