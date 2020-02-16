namespace PoshCommander

open PoshCommander
open PoshCommander.Colors
open System.Management.Automation.Host

type Pane = {
    CurrentDirectory: DirectoryContent
    FirstVisibleIndex: int
    HighlightedIndex: int
    IsActive: bool
    RowCount: int
    }

module Pane =
    let create rowCount isActive path =
        {
            CurrentDirectory = FileSystem.readDirectory path
            FirstVisibleIndex = 0
            HighlightedIndex = 0
            IsActive = isActive
            RowCount = rowCount
        }

    let getHighlightedItem pane =
        pane.CurrentDirectory.Items.[pane.HighlightedIndex]

    let tryGetItem index pane =
        if index >= 0 && index < pane.CurrentDirectory.Items.Count then
            Some pane.CurrentDirectory.Items.[index]
        else
            None

    let getItemCount pane =
        pane.CurrentDirectory.Items.Count

    let getLastItemIndex pane =
        if pane.CurrentDirectory.Items.Count > 0 then
            pane.CurrentDirectory.Items.Count - 1
        else
            invalidOp "Can't get last item index because directory is empty"

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
                match item.ItemType with
                | DirectoryItem -> Theme.directoryIcon
                | FileItem -> getFileIcon item.Name

            let itemIndex = index + paneState.FirstVisibleIndex
            if itemIndex = paneState.HighlightedIndex then
                drawHighlightedRow index icon item.Name
            else
                drawNormalRow index icon item.Name
        )

    let draw (ui: PSHostUserInterface) bounds paneState =
        let headerBounds = { bounds with Height = 1 }
        drawHeader ui headerBounds paneState

        let itemsBounds = { bounds with Top = 1; Height = bounds.Height - 1 }
        drawItems ui itemsBounds paneState

    let setHighlightedIndex index (pane: Pane) =
        let newIndex =
            if index < 0 then 0
            else if index > getLastItemIndex pane then getLastItemIndex pane
            else index

        if newIndex <> pane.HighlightedIndex then
            let newFirstVisibleIndex =
                if newIndex - pane.FirstVisibleIndex >= pane.RowCount then
                    newIndex - pane.RowCount + 1
                else if newIndex < pane.FirstVisibleIndex then
                    newIndex
                else
                    pane.FirstVisibleIndex

            { pane with
                FirstVisibleIndex = newFirstVisibleIndex
                HighlightedIndex = newIndex }
        else
            pane

    let highlightFirstItem pane = setHighlightedIndex 0 pane
    let highlightLastItem pane = setHighlightedIndex (getLastItemIndex pane) pane
    let highlightNextItem pane = setHighlightedIndex (pane.HighlightedIndex + 1) pane
    let highlightPreviousItem pane = setHighlightedIndex (pane.HighlightedIndex - 1) pane
    let highlightItemOnePageBefore pane = setHighlightedIndex (pane.HighlightedIndex - pane.RowCount + 1) pane
    let highlightItemOnePageAfter pane = setHighlightedIndex (pane.HighlightedIndex + pane.RowCount - 1) pane

    let navigateToDirectory directoryContent pane =
        let highlightedIndex =
                let originalDirectoryIndex =
                    directoryContent.Items
                    |> Seq.tryFindIndex (fun item -> item.FullPath = pane.CurrentDirectory.FullPath)
                match originalDirectoryIndex with
                | Some index -> index
                | None -> 0

        { pane with
            CurrentDirectory = directoryContent
            HighlightedIndex = highlightedIndex }

    let invokeHighlightedItem invokeDirectory invokeFile pane =
        let highlightedItem = getHighlightedItem pane
        match highlightedItem with
        | { ItemType = DirectoryItem } -> invokeDirectory highlightedItem pane
        | { ItemType = FileItem } -> invokeFile highlightedItem pane
