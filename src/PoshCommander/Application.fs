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
        let (leftPaneBounds, rightPaneBounds) =
            Rect.fromSize host.UI.RawUI.WindowSize
            |> Rect.splitHorizontally

        applicationState.LeftPane |> Pane.draw host.UI leftPaneBounds
        applicationState.RightPane |> Pane.draw host.UI rightPaneBounds

    let rec run host mapCommand applicationState =
        drawPanes host applicationState

        let keyInfo = Console.ReadKey(intercept = true)
        let maybeCommand = mapCommand keyInfo

        let newState =
            match maybeCommand with
            | Some command -> applicationState |> command
            | None -> applicationState

        if newState.IsRunning then
            run host mapCommand newState

    let switchActivePane application =
        let leftPane = application.LeftPane
        let rightPane = application.RightPane

        { application with
            LeftPane = { leftPane with IsActive = not leftPane.IsActive }
            RightPane = { rightPane with IsActive = not rightPane.IsActive } }

    let quit application =
        { application with IsRunning = false }
