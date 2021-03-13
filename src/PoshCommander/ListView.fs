module PoshCommander.ListView

open System

type Filter =
    | NoFilter
    | Filter of string

type Model<'TItem when 'TItem : comparison> =
    {
        FirstVisibleIndex: int
        HighlightedIndex: int option
        Items: ImmutableArray<'TItem>
        PageSize: int
        SelectedItems: Set<'TItem>
        VisibleItems: ImmutableArray<'TItem>
    }

let getFirstVisibleIndex model =
    model.FirstVisibleIndex

let getHighlightedIndex model =
    model.HighlightedIndex

let getHighlightedItem model =
    match getHighlightedIndex model with
    | Some index -> Some (ImmutableArray.get index model.Items)
    | None -> None

let getSelectedItems model =
    model.SelectedItems

let getVisibleItems model =
    model.VisibleItems

type Msg =
    | HighlightItemOnePageAfter
    | HighlightItemOnePageBefore
    | HighlightPreviousItem
    | HighlightNextItem
    | PageSizeChanged of int
    | ToggleItemSelection

let init pageSize items =
    {
        FirstVisibleIndex = 0
        HighlightedIndex = if ImmutableArray.isEmpty items then None else Some 0
        Items = items
        PageSize = pageSize
        SelectedItems = Set.empty
        VisibleItems = items
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

        let newFirstVisibleIndex =
            match newHighlightedIndex with
            | Some newIndexValue ->
                if newIndexValue < model.FirstVisibleIndex then
                    newIndexValue
                else if newIndexValue >= model.FirstVisibleIndex + model.PageSize then
                    newIndexValue - model.PageSize + 1
                else
                    model.FirstVisibleIndex
            | None ->
                model.FirstVisibleIndex

        {
            model with
                FirstVisibleIndex = newFirstVisibleIndex
                HighlightedIndex = newHighlightedIndex
        }

    let offsetHighlightedIndex offset model =
        model
        |> getHighlightedIndex
        |> Option.map (add offset)
        |> setHighlightedIndex model

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
        let colors = getItemColors (index + model.FirstVisibleIndex) false

        UI.setCursorPosition uiContext uiArea.Left (uiArea.Top + index)
        UI.drawFullLine uiContext colors ""

    let drawItem index item =
        let isSelected = Set.contains item model.SelectedItems
        let label = itemPresenter item
        let colors = getItemColors (index + model.FirstVisibleIndex) isSelected

        UI.setCursorPosition uiContext uiArea.Left (uiArea.Top + index)
        UI.drawFullLine uiContext colors label

    let drawRow index row =
        match row with
        | EmptyRow -> drawEmptyRow index
        | ItemRow item -> drawItem index item

    let emptyRows =
        Seq.initInfinite (fun _ -> EmptyRow)

    model.Items
    |> Seq.skip model.FirstVisibleIndex
    |> Seq.map ItemRow
    |> Seq.concatWith emptyRows
    |> Seq.take model.PageSize
    |> Seq.iteri drawRow
