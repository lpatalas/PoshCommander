namespace PoshCommander

open System
open System.Management.Automation
open PoshCommander.Input

// type UserAction =
//     | Command of (Pane -> Pane)
//     | Exit
//     | NoAction

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

    override this.EndProcessing() =
        FullScreenConsole.enter this.Host.UI.RawUI this.TestListView

    // member private this.RunApplication() =
    //     let windowSize = this.Host.UI.RawUI.WindowSize
    //     let application = Application.create windowSize this.LeftPath this.RightPath
    //     let draw = UI.drawApplication this.Host
    //     let mapCommand = Command.mapKeyToCommand Command.defaultKeyMap
    //     Application.run draw mapCommand application

    member private this.TestInput() =
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

    member private this.TestListView() =
        let flip f a b = f b a

        let items =
            Seq.init 100 (fun i -> sprintf "Item %d" i)
            |> Seq.toArray

        let itemPresenter item =
            item

        let pageSize = this.Host.UI.RawUI.WindowSize.Height
        let initialModel = ListView.init pageSize itemPresenter items

        let rec mainLoop model =
            let width = this.Host.UI.RawUI.WindowSize.Width / 2
            let height = this.Host.UI.RawUI.WindowSize.Height
            let uiContext = UIContext.init (this.Host.UI) width height
            ListView.view uiContext model

            let keyInfo = Console.ReadKey(intercept = true)
            if keyInfo.Key <> ConsoleKey.Q then
                let maybeKeyMsg =
                    match keyInfo.Key with
                    | ConsoleKey.DownArrow -> Some ListView.HighlightNextItem
                    | ConsoleKey.PageDown -> Some ListView.HighlightItemOnePageAfter
                    | ConsoleKey.PageUp -> Some ListView.HighlightItemOnePageBefore
                    | ConsoleKey.UpArrow -> Some ListView.HighlightPreviousItem
                    | _ -> None

                let maybePageMsg =
                    let newPageSize = this.Host.UI.RawUI.WindowSize.Height
                    if newPageSize <> model.PageSize then
                        Some (ListView.PageSizeChanged newPageSize)
                    else
                        None

                let updatedModel =
                    [maybeKeyMsg; maybePageMsg]
                    |> Seq.choose id
                    |> Seq.fold (flip ListView.update) model

                mainLoop updatedModel

        mainLoop initialModel
