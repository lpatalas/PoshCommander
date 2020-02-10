module PoshCommander.Commands.ApplicationCommands

open PoshCommander

let switchActivePane (application: Application) =
    let leftPane = application.LeftPane
    let rightPane = application.RightPane

    { application with
        LeftPane = { leftPane with IsActive = not leftPane.IsActive }
        RightPane = { rightPane with IsActive = not rightPane.IsActive } }

let quitApplication (application: Application) =
    { application with IsRunning = false }
