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
