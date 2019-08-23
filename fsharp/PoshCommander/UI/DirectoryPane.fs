module PoshCommander.UI.DirectoryPane

open System.Management.Automation.Host
open System.IO

type DirectoryItemType = Directory | File

type DirectoryItem = {
    FullPath: string
    ItemType: DirectoryItemType
    Name: string
    }

type DirectoryPaneState = {
    DirectoryInfo: DirectoryInfo
    Items: DirectoryItem[]
    }

let openDirectory path =
    let getItemType (fileSystemInfo: FileSystemInfo) =
        if fileSystemInfo.Attributes.HasFlag(FileAttributes.Directory) then
            Directory
        else
            File
        
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
        DirectoryInfo = directoryInfo
        Items = items
    }

let drawHeader (ui: PSHostUserInterface) (bounds: Rectangle) paneState =
    let rawUI = ui.RawUI
    rawUI.CursorPosition <- new Coordinates(0, bounds.Top)
    ui.Write(paneState.DirectoryInfo.FullName)

let drawItems (ui: PSHostUserInterface) (bounds: Rectangle) paneState =
    let rawUI = ui.RawUI
    let maxVisibleItems = bounds.Bottom - bounds.Top + 1

    paneState.Items
    |> Seq.truncate maxVisibleItems
    |> Seq.iteri (fun index item ->
        let icon =
            match item.ItemType with
            | Directory -> "D"
            | File -> "F"

        rawUI.CursorPosition <- new Coordinates(0, bounds.Top + index)
        ui.Write(icon + " " + item.Name)
    )

let draw (ui: PSHostUserInterface) (bounds: Rectangle) paneState =
    let headerBounds = new Rectangle(bounds.Left, bounds.Top, bounds.Right, bounds.Top)
    drawHeader ui headerBounds paneState

    let itemsBounds = new Rectangle(bounds.Left, bounds.Top + 1, bounds.Right, bounds.Bottom)
    drawItems ui itemsBounds paneState
