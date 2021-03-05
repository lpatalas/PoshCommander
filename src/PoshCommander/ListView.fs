module PoshCommander.ListView

open System

type Filter =
    | NoFilter
    | Filter of string

type Model<'TItem when 'TItem : comparison> =
    {
        FirstVisibleIndex: int
        HighlightedIndex: int
        Filter: Filter
        Items: 'TItem array
        PageSize: int
        SelectedItems: Set<'TItem>
    }

let getFirstVisibleIndex model =
    model.FirstVisibleIndex

let getHighlightedIndex model =
    model.HighlightedIndex

let getHighlightedItem model =
    model.Items.[getHighlightedIndex model]

let getSelectedItems model =
    model.SelectedItems

type Msg =
    | HighlightItemOnePageAfter
    | HighlightItemOnePageBefore
    | HighlightPreviousItem
    | HighlightNextItem
    | PageSizeChanged of int
    | ResetFilter
    | SetFilter of string
    | ToggleItemSelection

let init pageSize items =
    {
        FirstVisibleIndex = 0
        HighlightedIndex = 0
        Filter = NoFilter
        Items = items
        PageSize = pageSize
        SelectedItems = Set.empty
    }

let private filterInitChars =
    [ 'a'..'z' ]
    @ [ 'A'..'Z' ]
    @ [ '0'..'9' ]
    @ [ ','; '.'; '_' ]
    |> Set.ofList

let private filterUpdateChars =
    filterInitChars
    |> Set.add ' '

let isFilterInitChar keyChar =
    Set.contains keyChar filterInitChars

let isFilterUpdateChar keyChar =
    Set.contains keyChar filterUpdateChars

let private eraseLastChar str =
    match str with
    | "" -> ""
    | s -> s.Substring(0, s.Length - 1)

let tryMapFilterMsg (keyInfo: ConsoleKeyInfo) model =
    match model.Filter with
    | NoFilter ->
        if isFilterInitChar keyInfo.KeyChar then
            Some (SetFilter (string keyInfo.KeyChar))
        else
            None
    | Filter filterString ->
        if isFilterUpdateChar keyInfo.KeyChar then
            Some (SetFilter (filterString + string keyInfo.KeyChar))
        else if keyInfo.Key = ConsoleKey.Backspace then
            Some (SetFilter (eraseLastChar filterString))
        else if keyInfo.Key = ConsoleKey.Escape then
            Some ResetFilter
        else
            None

let tryMapOtherMsg key =
    match key with
    | ConsoleKey.DownArrow -> Some HighlightNextItem
    | ConsoleKey.Spacebar -> Some ToggleItemSelection
    | ConsoleKey.PageDown -> Some HighlightItemOnePageAfter
    | ConsoleKey.PageUp -> Some HighlightItemOnePageBefore
    | ConsoleKey.UpArrow -> Some HighlightPreviousItem
    | _ -> None

let mapKey (keyInfo: ConsoleKeyInfo) model =
    tryMapFilterMsg keyInfo model
    |> Option.orElseWith (fun () -> tryMapOtherMsg keyInfo.Key)

let update msg model =
    let clampIndex index =
        if index < 0 then 0
        else if index >= model.Items.Length then model.Items.Length - 1
        else index

    let setHighlightedIndex index model =
        let newHighlightedIndex =
            clampIndex index

        let newFirstVisibleIndex =
            if newHighlightedIndex < model.FirstVisibleIndex then
                newHighlightedIndex
            else if newHighlightedIndex >= model.FirstVisibleIndex + model.PageSize then
                newHighlightedIndex - model.PageSize + 1
            else
                model.FirstVisibleIndex

        {
            model with
                FirstVisibleIndex = newFirstVisibleIndex
                HighlightedIndex = newHighlightedIndex
        }

    let apply2 a b f =
        f a b

    let toggleItemSelection model =
        let highlightedItem = getHighlightedItem model
        let updatedSet =
            if Set.contains highlightedItem model.SelectedItems then
                Set.remove highlightedItem model.SelectedItems
            else
                Set.add highlightedItem model.SelectedItems

        { model with SelectedItems = updatedSet }

    let updatePageSize newPageSize =
        if model.HighlightedIndex >= newPageSize then
            { model with HighlightedIndex = newPageSize - 1 }
        else
            model

    match msg with
    | HighlightItemOnePageAfter ->
        model |> setHighlightedIndex (model.HighlightedIndex + model.PageSize - 1)
    | HighlightItemOnePageBefore ->
        model |> setHighlightedIndex (model.HighlightedIndex - model.PageSize + 1)
    | HighlightPreviousItem ->
        model |> setHighlightedIndex (model.HighlightedIndex - 1)
    | HighlightNextItem ->
        model |> setHighlightedIndex (model.HighlightedIndex + 1)
    | PageSizeChanged newPageSize ->
        updatePageSize newPageSize
    | ResetFilter ->
        { model with Filter = NoFilter }
    | SetFilter filterString ->
        { model with Filter = Filter filterString }
    | ToggleItemSelection ->
        model |> toggleItemSelection

let view uiContext itemPresenter model =
    let uiArea = UIContext.getArea uiContext

    let getItemColors index item =
        let backgroundColor =
            if index = model.HighlightedIndex then
                Theme.RowHightlighedBackground
            else if index % 2 = 0 then
                Theme.RowEvenBackground
            else
                Theme.RowOddBackground

        let foregroundColor =
            if Set.contains item model.SelectedItems then
                Theme.ItemSelectedForeground
            else
                Theme.ItemNormalForeground

        (foregroundColor, backgroundColor)

    let drawItem index item =
        let label = itemPresenter item
        let colors = getItemColors (index + model.FirstVisibleIndex) item

        UI.setCursorPosition uiContext uiArea.Left (uiArea.Top + index)
        UI.drawFullLine uiContext colors label

    model.Items
    |> Seq.skip model.FirstVisibleIndex
    |> Seq.take model.PageSize
    |> Seq.iteri drawItem
