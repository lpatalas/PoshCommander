namespace PoshCommander

open System
open System.Management.Automation
open PoshCommander.UI
open PoshCommander.UI.Input
open System.IO
open System.Management.Automation.Host

[<Cmdlet(VerbsLifecycle.Invoke, "PoshCommander")>]
type PoshCommanderCmdlet() =
    inherit PSCmdlet()
    
    [<Parameter(Position = 0)>]
    member val LeftPath = String.Empty with get, set

    [<Parameter(Position = 1)>]
    member val RightPath = String.Empty with get, set

    [<Parameter>]
    member val EditorPath = String.Empty with get, set

    [<Parameter>]
    member val ViewerPath = String.Empty with get, set

    override this.BeginProcessing() =
        FullScreenConsole.enter this.Host.UI.RawUI (fun () ->
            this.testPane()
            Console.ReadKey(true) |> ignore
            )

    member this.testPane() =
        let pane = DirectoryPane.openDirectory "C:\\Program Files (x86)"

        let ui = this.Host.UI
        let rawUI = ui.RawUI
        let bounds = new Rectangle(0, 0, rawUI.WindowSize.Width - 1, rawUI.WindowSize.Height - 1)

        DirectoryPane.draw ui bounds pane

    member this.testInput() =
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
