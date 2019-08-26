namespace PoshCommander

type Rectangle = {
    Left: int
    Top: int
    Width: int
    Height: int
    }

type DirectoryItemType = DirectoryItem | FileItem

type DirectoryItem = {
    FullPath: string
    ItemType: DirectoryItemType
    Name: string
    }

type PaneState = {
    DirectoryPath: string
    FirstVisibleIndex: int
    HighlightedIndex: int
    IsActive: bool
    Items: DirectoryItem[]
    RowCount: int
    }

type ApplicationState = {
    IsRunning: bool
    LeftPane: PaneState
    RightPane: PaneState
    }

type ApplicationCommand = ApplicationState -> ApplicationState
