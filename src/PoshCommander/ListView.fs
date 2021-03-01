module PoshCommander.ListView

open System
open System.Management.Automation.Host

type ItemPresenter<'T> = ('T) -> string

type Model<'TItem> =
    {
        HighlightedIndex: int
        ItemPresenter: ItemPresenter<'TItem>
        Items: 'TItem array
        PageSize: int
    }

type Msg =
    | HighlightItemOnePageAfter
    | HighlightItemOnePageBefore
    | HighlightPreviousItem
    | HighlightNextItem
    | PageSizeChanged of int

let init pageSize itemPresenter items =
    {
        HighlightedIndex = 0
        ItemPresenter = itemPresenter
        Items = items
        PageSize = pageSize
    }

let mapKey key =
    match key with
    | ConsoleKey.DownArrow -> Some HighlightNextItem
    | ConsoleKey.PageDown -> Some HighlightItemOnePageAfter
    | ConsoleKey.PageUp -> Some HighlightItemOnePageBefore
    | ConsoleKey.UpArrow -> Some HighlightPreviousItem
    | _ -> None

let update msg model =
    let clampIndex index =
        if index < 0 then 0
        else if index >= model.Items.Length then model.Items.Length - 1
        else index

    let updatePageSize newPageSize =
        if model.HighlightedIndex >= newPageSize then
            { model with HighlightedIndex = newPageSize - 1 }
        else
            model

    match msg with
    | HighlightItemOnePageAfter ->
        { model with HighlightedIndex = clampIndex (model.HighlightedIndex + model.PageSize - 1) }
    | HighlightItemOnePageBefore ->
        { model with HighlightedIndex = clampIndex (model.HighlightedIndex - model.PageSize + 1) }
    | HighlightPreviousItem ->
        { model with HighlightedIndex = clampIndex (model.HighlightedIndex - 1) }
    | HighlightNextItem ->
        { model with HighlightedIndex = clampIndex (model.HighlightedIndex + 1) }
    | PageSizeChanged newPageSize ->
        updatePageSize newPageSize

let view uiContext model =
    let uiArea = UIContext.getArea uiContext
    let normalOddColors = Theme.ItemNormalForeground, Theme.RowOddBackground
    let normalEvenColors = Theme.ItemNormalForeground, Theme.RowEvenBackground
    let highlightedColors = Theme.ItemNormalForeground, Theme.RowHightlighedBackground

    let getItemColors index =
        if index = model.HighlightedIndex then
            highlightedColors
        else if index % 2 = 0 then
            normalEvenColors
        else
            normalOddColors

    let drawItem index item =
        let label = model.ItemPresenter item
        let colors = getItemColors index

        UI.setCursorPosition uiContext uiArea.Left (uiArea.Top + index)
        UI.drawFullLine uiContext colors label

    let areaHeight = UIContext.getAreaHeight uiContext

    model.Items
    |> Seq.take model.PageSize
    |> Seq.iteri drawItem
