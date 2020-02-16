namespace PoshCommander

open System.Collections.Generic
open System.IO

type ItemType =
    | DirectoryItem
    | FileItem

type Item =
    {
        FullPath: string
        ItemType: ItemType
        Name: string
    }

type DirectoryContent =
    {
        FullPath: string
        Items: IReadOnlyList<Item>
        Name: string
    }

module FileSystem =
    let readDirectory path =
        let mapToItem (info: FileSystemInfo) =
            {
                FullPath = info.FullName
                ItemType =
                    if info.Attributes.HasFlag(FileAttributes.Directory) then DirectoryItem
                    else FileItem
                Name = info.Name
            }

        let directoryInfo = DirectoryInfo(path)
        let directories = directoryInfo.EnumerateDirectories() :?> IEnumerable<FileSystemInfo>
        let files = directoryInfo.EnumerateFiles() :?> IEnumerable<FileSystemInfo>

        let allItems =
            directories
            |> Seq.append files
            |> Seq.map mapToItem
            |> Seq.toArray

        {
            FullPath = directoryInfo.FullName
            Items = allItems
            Name = directoryInfo.Name
        }



