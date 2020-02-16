﻿namespace PoshCommander

open System
open System.Management.Automation
open PoshCommander.UI.Input

type UserAction =
    | Command of (Pane -> Pane)
    | Exit
    | NoAction

[<Cmdlet(VerbsLifecycle.Invoke, "PoshCommander")>]
type PoshCommanderCmdlet() =
    inherit PSCmdlet()

    [<Parameter(Position = 0)>]
    member val LeftPath = "C:\\Program Files (x86)" with get, set

    [<Parameter(Position = 1)>]
    member val RightPath = "C:\\Program Files" with get, set

    [<Parameter>]
    member val EditorPath = String.Empty with get, set

    [<Parameter>]
    member val ViewerPath = String.Empty with get, set

    override this.BeginProcessing() =
        FullScreenConsole.enter this.Host.UI.RawUI this.TestPane

    member this.TestPane() =
        let windowSize = this.Host.UI.RawUI.WindowSize
        let application = Application.create windowSize this.LeftPath this.RightPath

        let navigateToItem (item: Item) pane =
            let content = FileSystem.readDirectory item.FullPath
            Pane.navigateToDirectory content pane

        let invokeFile item pane =
            pane

        let invokeHighlightedItem =
            Pane.invokeHighlightedItem navigateToItem invokeFile

        let mapCommand (keyInfo: ConsoleKeyInfo) =
            let applyToActivePane paneCommand application =
                if application.LeftPane.IsActive then
                    { application with LeftPane = application.LeftPane |> paneCommand }
                else
                    { application with RightPane = application.RightPane |> paneCommand }

            match keyInfo.Key with
            | ConsoleKey.DownArrow -> Some (Pane.highlightNextItem |> applyToActivePane)
            | ConsoleKey.End -> Some (Pane.highlightLastItem |> applyToActivePane)
            | ConsoleKey.Enter -> Some (invokeHighlightedItem |> applyToActivePane)
            | ConsoleKey.Escape -> Some Application.quit
            | ConsoleKey.Home -> Some (Pane.highlightFirstItem |> applyToActivePane)
            | ConsoleKey.PageDown -> Some (Pane.highlightItemOnePageAfter |> applyToActivePane)
            | ConsoleKey.PageUp -> Some (Pane.highlightItemOnePageBefore |> applyToActivePane)
            | ConsoleKey.Tab -> Some Application.switchActivePane
            | ConsoleKey.UpArrow -> Some (Pane.highlightPreviousItem |> applyToActivePane)
            | _ -> None

        application |> Application.run this.Host mapCommand

    member this.TestInput() =
        let setCursorX x =
            let mutable cursorPos = this.Host.UI.RawUI.CursorPosition
            cursorPos.X <- x
            this.Host.UI.RawUI.CursorPosition <- cursorPos

        let updatePrompt input =
            let oldCursorX = this.Host.UI.RawUI.CursorPosition.X
            setCursorX 0
            this.Host.UI.Write("Input: " + input)
            let windowWidth = this.Host.UI.RawUI.WindowSize.Width
            let cursorX = this.Host.UI.RawUI.CursorPosition.X
            if oldCursorX > cursorX then
                this.Host.UI.Write(new string(' ', oldCursorX - cursorX))
                setCursorX cursorX

        let mutable isDone = false

        while not isDone do
            let result = readConsoleInput updatePrompt
            this.Host.UI.WriteLine()
            match result with
            | Some "quit" ->
                this.Host.UI.WriteLine("You typed quit")
                isDone <- true
            | Some input ->
                this.Host.UI.WriteLine("You entered: " + input)
            | None ->
                this.Host.UI.WriteLine("You cancelled")
