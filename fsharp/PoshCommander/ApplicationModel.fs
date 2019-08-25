namespace PoshCommander

open System.IO
open System.Management.Automation.Host

type DirectoryItemType = DirectoryItem | FileItem

type DirectoryItem = {
    FullPath: string
    ItemType: DirectoryItemType
    Name: string
    }

type PaneState = {
    Bounds: Rectangle
    DirectoryInfo: DirectoryInfo
    FirstVisibleIndex: int
    HighlightedIndex: int
    IsActive: bool
    Items: DirectoryItem[]
    }

type ApplicationState = {
    IsRunning: bool
    LeftPane: PaneState
    RightPane: PaneState
    }

type ApplicationCommand = ApplicationState -> ApplicationState
