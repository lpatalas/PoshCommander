namespace PoshCommander

type Pane = {
    CurrentDirectory: DirectoryContent
    FirstVisibleIndex: int
    HighlightedIndex: int
    IsActive: bool
    RowCount: int
    }

module Pane =
    let create rowCount isActive path =
        {
            CurrentDirectory = FileSystem.readDirectory path
            FirstVisibleIndex = 0
            HighlightedIndex = 0
            IsActive = isActive
            RowCount = rowCount
        }

    let getHighlightedItem pane =
        pane.CurrentDirectory.Items.[pane.HighlightedIndex]

    let tryGetItem index pane =
        if index >= 0 && index < pane.CurrentDirectory.Items.Count then
            Some pane.CurrentDirectory.Items.[index]
        else
            None

    let getItemCount pane =
        pane.CurrentDirectory.Items.Count

    let getLastItemIndex pane =
        if pane.CurrentDirectory.Items.Count > 0 then
            pane.CurrentDirectory.Items.Count - 1
        else
            invalidOp "Can't get last item index because directory is empty"

    let setHighlightedIndex index (pane: Pane) =
        let newIndex =
            if index < 0 then 0
            else if index > getLastItemIndex pane then getLastItemIndex pane
            else index

        if newIndex <> pane.HighlightedIndex then
            let newFirstVisibleIndex =
                if newIndex - pane.FirstVisibleIndex >= pane.RowCount then
                    newIndex - pane.RowCount + 1
                else if newIndex < pane.FirstVisibleIndex then
                    newIndex
                else
                    pane.FirstVisibleIndex

            { pane with
                FirstVisibleIndex = newFirstVisibleIndex
                HighlightedIndex = newIndex }
        else
            pane

    let highlightFirstItem pane =
        setHighlightedIndex 0 pane

    let highlightLastItem pane =
        setHighlightedIndex (getLastItemIndex pane) pane

    let highlightNextItem pane =
        setHighlightedIndex (pane.HighlightedIndex + 1) pane

    let highlightPreviousItem pane =
        setHighlightedIndex (pane.HighlightedIndex - 1) pane

    let highlightItemOnePageBefore pane =
        setHighlightedIndex (pane.HighlightedIndex - pane.RowCount + 1) pane

    let highlightItemOnePageAfter pane =
        setHighlightedIndex (pane.HighlightedIndex + pane.RowCount - 1) pane

    let setCurrentDirectory directoryContent pane =
        let highlightedIndex =
                let originalDirectoryIndex =
                    directoryContent.Items
                    |> Seq.tryFindIndex (fun item -> item.FullPath = pane.CurrentDirectory.FullPath)
                match originalDirectoryIndex with
                | Some index -> index
                | None -> 0

        { pane with
            CurrentDirectory = directoryContent
            HighlightedIndex = highlightedIndex }

    let invokeHighlightedItem invokeDirectory invokeFile pane =
        let highlightedItem = getHighlightedItem pane
        match highlightedItem with
        | { ItemType = DirectoryItem } -> invokeDirectory highlightedItem pane
        | { ItemType = FileItem } -> invokeFile highlightedItem pane
