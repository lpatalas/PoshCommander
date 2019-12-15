namespace PoshCommander

type Rectangle = {
    Left: int
    Top: int
    Width: int
    Height: int
    }

type FileSystemItemType = DirectoryItem | FileItem

type FileSystemItem = {
    FullPath: string
    ItemType: FileSystemItemType
    Name: string
    }

type PaneState = {
    DirectoryPath: string
    FirstVisibleIndex: int
    HighlightedIndex: int
    IsActive: bool
    Items: FileSystemItem[]
    RowCount: int
    }

type ApplicationState = {
    IsRunning: bool
    LeftPane: PaneState
    RightPane: PaneState
    }

type ApplicationCommand = ApplicationState -> ApplicationState
