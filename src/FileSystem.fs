namespace PoshCommander

open System
open System.IO

type DirectoryItemType =
    | Directory
    | File

type DirectoryItem =
    {
        ItemType: DirectoryItemType
        Name: string
        Path: string
    }

module DirectoryItem =
    let fromFileSystemInfo (info: FileSystemInfo) =
        {
            Name = info.Name
            Path = info.FullName
            ItemType =
                if info.Attributes.HasFlag(FileAttributes.Directory) then Directory
                else File
        }

    let getName item =
        item.Name

    let getPath item =
        item.Path

    let isDirectory item =
        item.ItemType = Directory

    let isFile item =
        item.ItemType = File

type DirectoryContent =
    | DirectoryContent of ImmutableArray<DirectoryItem>
    | DirectoryAccessDenied
    | DirectoryNotFound
    | DirectoryReadError

type Directory =
    {
        Content: DirectoryContent
        Name: string
        ParentPath: string option
        Path: string
    }

module Directory =
    let read path =
        let readContent (directoryInfo: DirectoryInfo) =
            try
                directoryInfo.EnumerateFileSystemInfos()
                |> Seq.map DirectoryItem.fromFileSystemInfo
                |> Seq.sortBy DirectoryItem.isFile
                |> ImmutableArray.fromSeq
                |> DirectoryContent
            with
            | :? DirectoryNotFoundException ->
                DirectoryNotFound
            | :? UnauthorizedAccessException ->
                DirectoryAccessDenied
            | _ ->
                DirectoryReadError

        if String.IsNullOrEmpty(path) then
            {
                Content = DirectoryNotFound
                Name = String.Empty
                ParentPath = None
                Path = String.Empty
            }
        else
            let directoryInfo = DirectoryInfo(path)
            let parentPath =
                let hasParent =
                    not (isNull directoryInfo.Parent)
                    && not (String.IsNullOrEmpty(directoryInfo.Parent.FullName))

                if hasParent then
                    Some directoryInfo.Parent.FullName
                else
                    None

            {
                Content = readContent directoryInfo
                Name = directoryInfo.Name
                ParentPath = parentPath
                Path = directoryInfo.FullName
            }





