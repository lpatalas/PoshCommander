namespace PoshCommander

open System.Management.Automation.Host

module UI =
    let ansiEsc = '\x1b'
    let ansiResetCode = "\x1b[0m"

    let toAnsiColorCode layerCode (RgbColor (r, g, b)) =
        sprintf "%c[%i;2;%i;%i;%im" ansiEsc layerCode r g b

    let toAnsiBgColorCode = toAnsiColorCode 48
    let toAnsiFgColorCode = toAnsiColorCode 38

    let drawHeader (ui: PSHostUserInterface) bounds paneState =
        let rawUI = ui.RawUI
        rawUI.CursorPosition <- Coordinates(bounds.Left, 1000 - ui.RawUI.WindowSize.Height +  bounds.Top)

        let (bgColor, fgColor) =
            match paneState.IsActive with
            | true -> (Theme.TitleBarActiveBackground, Theme.TitleBarActiveForeground)
            | false -> (Theme.TitleBarInactiveBackground, Theme.TitleBarInactiveForeground)

        let ansiBgColor = bgColor |> toAnsiBgColorCode
        let ansiFgColor = fgColor |> toAnsiFgColorCode

        let caption = paneState.CurrentDirectory.FullPath
        let padding = new string(' ', bounds.Width - caption.Length)
        let styledText = ansiBgColor + ansiFgColor + caption + padding + ansiResetCode
        ui.Write(styledText)

    let drawItems (ui: PSHostUserInterface) bounds paneState =
        let getBgColor rowIndex =
            if rowIndex + paneState.FirstVisibleIndex = paneState.HighlightedIndex then
                Theme.RowHightlighedBackground
            else if rowIndex % 2 = 0 then
                Theme.RowEvenBackground
            else
                Theme.RowOddBackground

        let getFgColor rowIndex =
            if rowIndex = paneState.HighlightedIndex then
                Theme.ItemSelectedForeground
            else
                Theme.ItemNormalForeground

        let tryGetItem rowIndex =
            let itemIndex = rowIndex + paneState.FirstVisibleIndex
            if itemIndex < paneState.CurrentDirectory.Items.Count then
                Some paneState.CurrentDirectory.Items.[itemIndex]
            else
                None

        let drawItemRow bgColor fgColor item =
            let bgCode = bgColor |> toAnsiBgColorCode
            let fgCode = fgColor |> toAnsiFgColorCode
            let icon =
                match item with
                | DirectoryItem _ -> Theme.directoryIcon
                | FileItem file -> Theme.getFileIcon Theme.defaultFileIcon Theme.defaultFilePatterns file.FileName

            let itemName = Item.getName item
            let iconFgCode = toAnsiFgColorCode icon.Color
            let coloredIcon = sprintf "%s%c " iconFgCode icon.Glyph
            let coloredName = fgCode + itemName
            let padding = new string(' ', bounds.Width - itemName.Length - 2)
            ui.Write(bgCode + coloredIcon + coloredName + padding + ansiResetCode)

        let drawEmptyRow bgColor =
            let bgCode = bgColor |> toAnsiBgColorCode
            let padding = new string(' ', bounds.Width)
            ui.Write(bgCode + padding + ansiResetCode)

        let drawRow rowIndex =
            ui.RawUI.CursorPosition <- Coordinates(bounds.Left, 1000 - ui.RawUI.WindowSize.Height + bounds.Top + rowIndex)

            let bgColor = getBgColor rowIndex
            let fgColor = getFgColor rowIndex

            match tryGetItem rowIndex with
            | Some item -> drawItemRow bgColor fgColor item
            | None -> drawEmptyRow bgColor

        for rowIndex in 0..(bounds.Height - 1) do
            drawRow rowIndex

    let drawPane (ui: PSHostUserInterface) bounds paneState =
        let headerBounds = { bounds with Height = 1 }
        drawHeader ui headerBounds paneState

        let itemsBounds = { bounds with Top = 1; Height = bounds.Height - 1 }
        drawItems ui itemsBounds paneState

    let drawApplication (host: PSHost) application =
        let (leftPaneBounds, rightPaneBounds) =
            Rect.fromSize host.UI.RawUI.WindowSize
            |> Rect.splitHorizontally

        application.LeftPane |> drawPane host.UI leftPaneBounds
        application.RightPane |> drawPane host.UI rightPaneBounds
