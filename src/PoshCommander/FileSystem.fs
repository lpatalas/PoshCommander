namespace PoshCommander

open System
open System.Collections.Generic
open System.IO

type ItemType =
    | DirectoryItem
    | FileItem

type Item =
    {
        ItemType: ItemType
        Name: string
        Path: string
    }

module Item =
    let fromFileSystemInfo (info: FileSystemInfo) =
        {
            Name = info.Name
            Path = info.FullName
            ItemType =
                if info.Attributes.HasFlag(FileAttributes.Directory) then DirectoryItem
                else FileItem
        }

    let getPath item =
        item.Path

    let isDirectory item =
        item.ItemType = DirectoryItem

    let isFile item =
        item.ItemType = FileItem

// type DirectoryItem =
//     {
//         DirectoryPath: string
//         DirectoryName: string
//     }

// module DirectoryItem =
//     let fromDirectoryInfo (info: DirectoryInfo) =
//         {
//             DirectoryName = info.Name
//             DirectoryPath = info.FullName
//         }

//     let fromPath path =
//         {
//             DirectoryName = Path.GetFileName(path)
//             DirectoryPath = path
//         }


// type FileItem =
//     {
//         FilePath: string
//         FileName: string
//     }

// type Item =
//     | DirectoryItem of DirectoryItem
//     | FileItem of FileItem

// module ItemEx =
//     let createDirectory name path =
//         {
//             DirectoryName = name
//             DirectoryPath = path
//         }
//         |> DirectoryItem

//     let createFile name path =
//         {
//             FileName = name
//             FilePath = path
//         }
//         |> FileItem

//     let getName item =
//         match item with
//         | DirectoryItem d -> d.DirectoryName
//         | FileItem f -> f.FileName

//     let getPath item =
//         match item with
//         | DirectoryItem d -> d.DirectoryPath
//         | FileItem f -> f.FilePath

//     let isDirectory item =
//         match item with
//         | DirectoryItem _ -> true
//         | _ -> false

//     let isFile item =
//         match item with
//         | FileItem _ -> true
//         | _ -> false

type DirectoryContent =
    {
        FullPath: string
        Items: IReadOnlyList<Item>
        Name: string
    }

module FileSystem =
    let tryGetParentDirectoryPath path =
        let parentPath = Path.GetDirectoryName(path)
        if String.IsNullOrEmpty(parentPath) then
            None
        else
            Some parentPath

    let readDirectory path =
        let directoryInfo = DirectoryInfo(path)

        let childDirectories =
            directoryInfo.EnumerateDirectories()
            :?> IEnumerable<FileSystemInfo>

        let files =
            directoryInfo.EnumerateFiles()
            :?> IEnumerable<FileSystemInfo>

        let allItems =
            childDirectories
            |> Seq.append files
            |> Seq.map Item.fromFileSystemInfo
            |> Seq.toArray

        {
            FullPath = directoryInfo.FullName
            Items = allItems
            Name = directoryInfo.Name
        }



