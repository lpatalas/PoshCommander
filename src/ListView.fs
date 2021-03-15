module PoshCommander.ListView

open System

type Model<'TItem when 'TItem : comparison> =
    {
        HighlightedIndex: int option
        Items: ImmutableArray<'TItem>
        PageSize: int
        ScrollIndex: int
        SelectedItems: Set<'TItem>
    }

let getScrollIndex model =
    model.ScrollIndex

let getHighlightedIndex model =
    model.HighlightedIndex

let getHighlightedItem model =
    match getHighlightedIndex model with
    | Some index -> Some (ImmutableArray.get index model.Items)
    | None -> None

let getItems model =
    model.Items

let getSelectedItems model =
    model.SelectedItems

type Msg<'TItem when 'TItem : equality> =
    | HighlightItemOnePageAfter
    | HighlightItemOnePageBefore
    | HighlightPreviousItem
    | HighlightNextItem
    | PageSizeChanged of int
    | SetItems of ImmutableArray<'TItem>
    | ToggleItemSelection

let init pageSize items =
    {
        HighlightedIndex = if ImmutableArray.isEmpty items then None else Some 0
        Items = items
        PageSize = pageSize
        ScrollIndex = 0
        SelectedItems = Set.empty
    }

let mapKey (keyInfo: ConsoleKeyInfo) model =
    match keyInfo.Key with
    | ConsoleKey.DownArrow -> Some HighlightNextItem
    | ConsoleKey.Spacebar -> Some ToggleItemSelection
    | ConsoleKey.PageDown -> Some HighlightItemOnePageAfter
    | ConsoleKey.PageUp -> Some HighlightItemOnePageBefore
    | ConsoleKey.UpArrow -> Some HighlightPreviousItem
    | _ -> None

let update msg model =
    let inline add a b =
        a + b

    let clampIndex index =
        if index < 0 then 0
        else if index >= model.Items.Length then model.Items.Length - 1
        else index

    let setHighlightedIndex model index =
        let newHighlightedIndex =
            index |> Option.map clampIndex

        let newScrollIndex =
            match newHighlightedIndex with
            | Some newIndexValue ->
                if newIndexValue < model.ScrollIndex then
                    newIndexValue
                else if newIndexValue >= model.ScrollIndex + model.PageSize then
                    newIndexValue - model.PageSize + 1
                else
                    model.ScrollIndex
            | None ->
                model.ScrollIndex

        {
            model with
                ScrollIndex = newScrollIndex
                HighlightedIndex = newHighlightedIndex
        }

    let offsetHighlightedIndex offset model =
        model
        |> getHighlightedIndex
        |> Option.map (add offset)
        |> setHighlightedIndex model

    let setItems newItems model =
        if model.Items <> newItems then
            let newHighlightedIndex =
                getHighlightedItem model
                |> Option.bind (fun item -> Seq.tryFindItemIndex item newItems)
                |> Option.orElse (
                    if ImmutableArray.isEmpty newItems then None
                    else Some 0
                    )

            let newSelectedItems =
                newItems
                |> Seq.filter (fun item -> Set.contains item model.SelectedItems)
                |> Set.ofSeq

            { model with
                HighlightedIndex = newHighlightedIndex
                Items = newItems
                SelectedItems = newSelectedItems }
        else
            model

    let toggleItemSelection model =
        match getHighlightedItem model with
        | Some highlightedItem ->
            let updatedSet =
                if Set.contains highlightedItem model.SelectedItems then
                    Set.remove highlightedItem model.SelectedItems
                else
                    Set.add highlightedItem model.SelectedItems

            { model with SelectedItems = updatedSet }
        | None ->
            model

    let updatePageSize newPageSize model =
        match getHighlightedIndex model with
        | Some highlightedIndex ->
            if highlightedIndex >= newPageSize then
                { model with HighlightedIndex = Some (newPageSize - 1) }
            else
                model
        | None ->
            model

    model |>
        match msg with
        | HighlightItemOnePageAfter ->
            offsetHighlightedIndex (model.PageSize - 1)
        | HighlightItemOnePageBefore ->
            offsetHighlightedIndex (-model.PageSize + 1)
        | HighlightPreviousItem ->
            offsetHighlightedIndex (-1)
        | HighlightNextItem ->
            offsetHighlightedIndex 1
        | PageSizeChanged newPageSize ->
            updatePageSize newPageSize
        | SetItems newItems ->
            setItems newItems
        | ToggleItemSelection ->
            toggleItemSelection

[<Struct>]
type private ListViewRow<'TItem> =
    | EmptyRow
    | ItemRow of 'TItem

let view uiContext itemPresenter model =
    let uiArea = UIContext.getArea uiContext

    let getItemColors index isSelected =
        let backgroundColor =
            if Some index = getHighlightedIndex model then
                Theme.RowHightlighedBackground
            else if index % 2 = 0 then
                Theme.RowEvenBackground
            else
                Theme.RowOddBackground

        let foregroundColor =
            if isSelected then
                Theme.ItemSelectedForeground
            else
                Theme.ItemNormalForeground

        (foregroundColor, backgroundColor)

    let drawEmptyRow index =
        let colors = getItemColors (index + model.ScrollIndex) false

        UI.setCursorPosition uiContext uiArea.Left (uiArea.Top + index)
        UI.drawFullLine uiContext colors ""

    let drawItem index item =
        let isSelected = Set.contains item model.SelectedItems
        let label = itemPresenter item
        let colors = getItemColors (index + model.ScrollIndex) isSelected

        UI.setCursorPosition uiContext uiArea.Left (uiArea.Top + index)
        UI.drawFullLine uiContext colors label

    let drawRow index row =
        match row with
        | EmptyRow -> drawEmptyRow index
        | ItemRow item -> drawItem index item

    let emptyRows =
        Seq.initInfinite (fun _ -> EmptyRow)

    model.Items
    |> Seq.skip model.ScrollIndex
    |> Seq.map ItemRow
    |> Seq.concatWith emptyRows
    |> Seq.take model.PageSize
    |> Seq.iteri drawRow

let fromAsciiArt itemFromString input =
    let lines =
        String.splitLines input
        |> Array.map String.trimWhitespace

    let highlightedIndex =
        lines
        |> Array.tryFindIndex (String.startsWith ">")

    let scrollIndex =
        lines
        |> Array.tryFindIndex (String.endsWith "|")
        |> Option.defaultValue 0

    let pageSize =
        lines
        |> Array.tryFindIndexBack (String.endsWith "|")
        |> Option.map (fun index -> index - scrollIndex + 1)
        |> Option.defaultValue lines.Length

    let parseItem input =
        let isSelected = String.startsWith "[" input
        let value =
            input
            |> String.trim "[]"
            |> itemFromString

        (value, isSelected)

    let itemData =
        lines
        |> Seq.filter (not << String.IsNullOrEmpty)
        |> Seq.map (String.trim " >|")
        |> Seq.map parseItem
        |> ImmutableArray.fromSeq

    let items =
        itemData
        |> ImmutableArray.map fst

    let selectedItems =
        itemData
        |> Seq.filter snd
        |> Seq.map fst
        |> Set.ofSeq

    {
        ScrollIndex = scrollIndex
        HighlightedIndex = highlightedIndex
        Items = items
        PageSize = pageSize
        SelectedItems = selectedItems
    }

let toAsciiArt itemToString model =
    let getItemText maybeItem =
        match maybeItem with
        | Some item -> itemToString item
        | None -> String.Empty

    let applyHighlight index =
        if Some index = model.HighlightedIndex then
            sprintf "> %s"
        else
            sprintf "  %s"

    let applySelection maybeItem =
        match maybeItem with
        | Some item ->
            if Set.contains item model.SelectedItems then
                sprintf "[%s]"
            else
                sprintf " %s "
        | None ->
            sprintf " %s "

    let applyScroll index =
        let lastVisibleIndex = model.ScrollIndex + model.PageSize - 1
        if index >= model.ScrollIndex && index <= lastVisibleIndex then
            sprintf "%s |"
        else
            sprintf "%s  "

    let applyPadding maxLength item =
        let paddingCount = maxLength - String.length item
        if paddingCount > 0 then
            sprintf "%s%s" item (String (' ', paddingCount))
        else
            item

    let formatItem maxLabelLength index item =
        getItemText item
        |> applyPadding maxLabelLength
        |> applySelection item
        |> applyHighlight index
        |> applyScroll index

    let maxLabelLength =
        model.Items
        |> Seq.map (itemToString >> String.length)
        |> Seq.tryMax
        |> Option.defaultValue 0

    seq { 0..(Math.Max(model.PageSize, ImmutableArray.length model.Items)) }
    |> Seq.map (fun i -> ImmutableArray.tryGet i model.Items)
    |> Seq.mapi (formatItem maxLabelLength)
    |> Seq.fold (fun s1 s2 -> String.concat "\n" [|s1; s2|]) ""
