module PoshCommander.FullScreenConsole

open System
open System.Management.Automation.Host

type private Snapshot = {
    BackgroundColor: ConsoleColor
    BufferContents: BufferCell[,]
    CursorPosition: Coordinates
    CursorSize: int
    ForegroundColor: ConsoleColor
    }

let enter (rawUI: PSHostRawUserInterface) (callback: unit -> unit) =
    let bufferSize = rawUI.BufferSize

    let takeSnapshot () =
        let bufferContents =
            let bufferRect = new Rectangle(0, 0, bufferSize.Width - 1, bufferSize.Height - 1)
            rawUI.GetBufferContents(bufferRect)
    
        {
            BackgroundColor = rawUI.BackgroundColor
            BufferContents = bufferContents
            CursorPosition = rawUI.CursorPosition
            CursorSize = rawUI.CursorSize
            ForegroundColor = rawUI.ForegroundColor
        }

    let resetScreen () =
        let windowSize = rawUI.WindowSize
        rawUI.WindowPosition <- new Coordinates(0, bufferSize.Height - windowSize.Height)
        rawUI.CursorPosition <- new Coordinates(0, bufferSize.Height - windowSize.Height)
        rawUI.CursorSize <- 0

    let restoreSnapshot snapshot =
        rawUI.SetBufferContents(new Coordinates(0, 0), snapshot.BufferContents)
        rawUI.BackgroundColor <- snapshot.BackgroundColor
        rawUI.CursorPosition <- snapshot.CursorPosition
        rawUI.CursorSize <- snapshot.CursorSize
        rawUI.ForegroundColor <- snapshot.ForegroundColor

    let snapshot = takeSnapshot()
    resetScreen()

    try
        callback()
    finally
        restoreSnapshot snapshot
