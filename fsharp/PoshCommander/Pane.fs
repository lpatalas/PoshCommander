module PoshCommander.Pane

open PoshCommander
open PoshCommander.Colors
open System.IO
open System.Management.Automation.Host

let create bounds isActive path =
    let getItemType (fileSystemInfo: FileSystemInfo) =
        if fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory) then
            DirectoryItem
        else
            FileItem
        
    let createEntry (fileSystemInfo: FileSystemInfo) =
        {
            FullPath = fileSystemInfo.FullName
            ItemType = getItemType fileSystemInfo
            Name = fileSystemInfo.Name
        }

    let directoryInfo = new DirectoryInfo(path)
    let items =
        directoryInfo.EnumerateFileSystemInfos()
        |> Seq.map createEntry
        |> Seq.toArray

    {
        Bounds = bounds
        DirectoryInfo = directoryInfo
        FirstVisibleIndex = 0
        HighlightedIndex = 0
        IsActive = isActive
        Items = items
    }
    
let drawHeader (ui: PSHostUserInterface) (bounds: Rectangle) paneState =
    let rawUI = ui.RawUI
    rawUI.CursorPosition <- new Coordinates(bounds.Left, bounds.Top)
    
    let (bgColor, fgColor) =
        match paneState.IsActive with
        | true -> (Theme.TitleBarActiveBackground, Theme.TitleBarActiveForeground)
        | false -> (Theme.TitleBarInactiveBackground, Theme.TitleBarInactiveForeground)
    
    let ansiBgColor = bgColor |> toAnsiBgColorCode
    let ansiFgColor = fgColor |> toAnsiFgColorCode
    
    let caption = paneState.DirectoryInfo.FullName
    let padding = new string(' ', bounds.Right - bounds.Left + 1 - caption.Length)
    let styledText = ansiBgColor + ansiFgColor + caption + padding + ansiResetCode
    ui.Write(styledText)
    
let drawItems (ui: PSHostUserInterface) (bounds: Rectangle) paneState =
    let rawUI = ui.RawUI
    let maxVisibleItems = bounds.Bottom - bounds.Top + 1
    let totalWidth = bounds.Right - bounds.Left + 1
    
    let drawRow bgColor fgColor index (text: string) =
        let bgCode = bgColor |> toAnsiBgColorCode
        let fgCode = fgColor |> toAnsiFgColorCode
        let padding = new string(' ', totalWidth - text.Length)
        rawUI.CursorPosition <- new Coordinates(bounds.Left, bounds.Top + index)
        ui.Write(bgCode + fgCode + text + padding + ansiResetCode)
    
    let drawHighlightedRow =
        drawRow Theme.RowHightlighedBackground Theme.ItemNormalForeground
    
    let drawNormalRow index =
        let bgColor =
            if index % 2 = 0 then
                Theme.RowEvenBackground
            else
                Theme.RowOddBackground
                
        drawRow bgColor Theme.ItemNormalForeground index
    
    paneState.Items
    |> Seq.skip paneState.FirstVisibleIndex
    |> Seq.truncate maxVisibleItems
    |> Seq.iteri (fun index item ->
        let icon =
            match item.ItemType with
            | DirectoryItem -> "D"
            | FileItem -> "F"
    
        let text = icon + " " + item.Name
        if index = paneState.HighlightedIndex then
            drawHighlightedRow index text
        else
            drawNormalRow index text
    )
    
let draw (ui: PSHostUserInterface) paneState =
    let bounds = paneState.Bounds
    
    let headerBounds = new Rectangle(bounds.Left, bounds.Top, bounds.Right, bounds.Top)
    drawHeader ui headerBounds paneState
    
    let itemsBounds = new Rectangle(bounds.Left, bounds.Top + 1, bounds.Right, bounds.Bottom)
    drawItems ui itemsBounds paneState
