namespace PoshCommander

open System

type Command =
    | HighlightFirstItem
    | HighlightItemOnePageAfter
    | HighlightItemOnePageBefore
    | HighlightLastItem
    | HighlightNextItem
    | HighlightPreviousItem
    | InvokeHighlightedItem
    | SwitchActivePane
    | QuitApplication

module Command =
    let defaultKeyMap =
        [|
            ConsoleKey.DownArrow, HighlightNextItem
            ConsoleKey.End, HighlightLastItem
            ConsoleKey.Enter, InvokeHighlightedItem
            ConsoleKey.Escape, QuitApplication
            ConsoleKey.Home, HighlightFirstItem
            ConsoleKey.PageDown, HighlightItemOnePageAfter
            ConsoleKey.PageUp, HighlightItemOnePageBefore
            ConsoleKey.Tab, SwitchActivePane
            ConsoleKey.UpArrow, HighlightPreviousItem
        |]
        |> Map.ofArray

    let executeCommand command application =
        let applyToActivePane paneCommand application =
            if application.LeftPane.IsActive then
                { application with LeftPane = application.LeftPane |> paneCommand }
            else
                { application with RightPane = application.RightPane |> paneCommand }

        let navigateToItem path pane =
            let content = FileSystem.readDirectory path
            Pane.setCurrentDirectory content pane

        let invokeFile _ pane =
            pane

        let invokeHighlightedItem =
            Pane.invokeHighlightedItem navigateToItem invokeFile

        let commandHandler =
            match command with
            | HighlightFirstItem ->
                Pane.highlightFirstItem |> applyToActivePane
            | HighlightItemOnePageAfter ->
                Pane.highlightItemOnePageAfter |> applyToActivePane
            | HighlightItemOnePageBefore ->
                Pane.highlightItemOnePageBefore |> applyToActivePane
            | HighlightLastItem ->
                Pane.highlightLastItem |> applyToActivePane
            | HighlightNextItem ->
                Pane.highlightNextItem |> applyToActivePane
            | HighlightPreviousItem ->
                Pane.highlightPreviousItem |> applyToActivePane
            | InvokeHighlightedItem ->
                invokeHighlightedItem |> applyToActivePane
            | SwitchActivePane ->
                Application.switchActivePane
            | QuitApplication ->
                Application.quit

        application |> commandHandler

    let mapKeyToCommand keyMap key =
        let maybeAction = Map.tryFind key keyMap
        match maybeAction with
        | Some command -> executeCommand command
        | None -> id
