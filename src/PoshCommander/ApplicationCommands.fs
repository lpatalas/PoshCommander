module PoshCommander.Commands.ApplicationCommands

open PoshCommander

let switchActivePane (application: ApplicationState) =
    let leftPane = application.LeftPane
    let rightPane = application.RightPane

    { application with
        LeftPane = { leftPane with IsActive = not leftPane.IsActive }
        RightPane = { rightPane with IsActive = not rightPane.IsActive } }

let quitApplication (application: ApplicationState) =
    { application with IsRunning = false }
