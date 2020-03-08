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
        let rawUI = ui.RawUI
        let totalWidth = bounds.Width

        let drawRow bgColor fgColor index (icon: Theme.Icon) (itemName: string) =
            let bgCode = bgColor |> toAnsiBgColorCode
            let fgCode = fgColor |> toAnsiFgColorCode
            let padding = new string(' ', totalWidth - itemName.Length - 2)
            rawUI.CursorPosition <- Coordinates(bounds.Left, 1000 - ui.RawUI.WindowSize.Height + bounds.Top + index)

            let iconFgCode = toAnsiFgColorCode icon.Color
            let coloredIcon = sprintf "%s%c " iconFgCode icon.Glyph
            let coloredName = fgCode + itemName
            ui.Write(bgCode + coloredIcon + coloredName + padding + ansiResetCode)

        let drawNormalRow index =
            let bgColor =
                if index % 2 = 0 then
                    Theme.RowEvenBackground
                else
                    Theme.RowOddBackground
            drawRow bgColor Theme.ItemNormalForeground index

        let drawHighlightedRow =
            if paneState.IsActive then
                drawRow Theme.RowHightlighedBackground Theme.ItemNormalForeground
            else
                drawNormalRow

        let getFileIcon =
            Theme.getFileIcon Theme.defaultFileIcon Theme.defaultFilePatterns

        paneState.CurrentDirectory.Items
        |> Seq.skip paneState.FirstVisibleIndex
        |> Seq.truncate paneState.RowCount
        |> Seq.iteri (fun index item ->
            let icon =
                match item with
                | DirectoryItem _ -> Theme.directoryIcon
                | FileItem file -> getFileIcon file.FileName

            let itemIndex = index + paneState.FirstVisibleIndex
            if itemIndex = paneState.HighlightedIndex then
                drawHighlightedRow index icon item.Name
            else
                drawNormalRow index icon item.Name
        )

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
