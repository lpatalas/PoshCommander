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

    let rec run draw mapCommand applicationState =
        draw applicationState

        let keyInfo = Console.ReadKey(intercept = true)
        let commandAction = mapCommand keyInfo.Key
        let newState = applicationState |> commandAction

        if newState.IsRunning then
            run draw mapCommand newState

    let switchActivePane application =
        let leftPane = application.LeftPane
        let rightPane = application.RightPane

        { application with
            LeftPane = { leftPane with IsActive = not leftPane.IsActive }
            RightPane = { rightPane with IsActive = not rightPane.IsActive } }

    let quit application =
        { application with IsRunning = false }
