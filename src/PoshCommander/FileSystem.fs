namespace PoshCommander

open System.Collections.Generic
open System.IO

type ItemType =
    | DirectoryItem
    | FileItem

type DirectoryItem =
    {
        DirectoryPath: string
        DirectoryName: string
    }

module DirectoryItem =
    let fromDirectoryInfo (info: DirectoryInfo) =
        {
            DirectoryName = info.Name
            DirectoryPath = info.FullName
        }

    let fromPath path =
        {
            DirectoryName = Path.GetFileName(path)
            DirectoryPath = path
        }


type FileItem =
    {
        FilePath: string
        FileName: string
    }

type Item =
    | DirectoryItem of DirectoryItem
    | FileItem of FileItem

module Item =
    let createDirectory name path =
        {
            DirectoryName = name
            DirectoryPath = path
        }
        |> DirectoryItem

    let createFile name path =
        {
            FileName = name
            FilePath = path
        }
        |> FileItem

    let getName item =
        match item with
        | DirectoryItem d -> d.DirectoryName
        | FileItem f -> f.FileName

    let getPath item =
        match item with
        | DirectoryItem d -> d.DirectoryPath
        | FileItem f -> f.FilePath

    let isDirectory item =
        match item with
        | DirectoryItem _ -> true
        | _ -> false

    let isFile item =
        match item with
        | FileItem _ -> true
        | _ -> false

type DirectoryContent =
    {
        FullPath: string
        Items: IReadOnlyList<Item>
        Name: string
    }

module FileSystem =
    let tryGetParentDirectoryPath directory =
        let directoryInfo = DirectoryInfo(directory.DirectoryPath)
        if not (isNull directoryInfo.Parent) then
            Some (DirectoryItem.fromDirectoryInfo directoryInfo.Parent)
        else
            None

    let readDirectory directory =
        let mapToItem (info: FileSystemInfo) =
            if info.Attributes.HasFlag(FileAttributes.Directory) then
                DirectoryItem { DirectoryName = info.Name; DirectoryPath = info.FullName }
            else
                FileItem { FileName = info.Name; FilePath = info.FullName }

        let directoryInfo = DirectoryInfo(directory.DirectoryPath)

        let childDirectories =
            directoryInfo.EnumerateDirectories()
            :?> IEnumerable<FileSystemInfo>

        let files =
            directoryInfo.EnumerateFiles()
            :?> IEnumerable<FileSystemInfo>

        let allItems =
            childDirectories
            |> Seq.append files
            |> Seq.map mapToItem
            |> Seq.toArray

        {
            FullPath = directoryInfo.FullName
            Items = allItems
            Name = directoryInfo.Name
        }



