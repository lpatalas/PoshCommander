module PoshCommander.ListView

open System
open System.Management.Automation.Host

type ItemPresenter<'T> = ('T) -> string

type Model<'TItem> =
    {
        HighlightedIndex: int
        ItemPresenter: ItemPresenter<'TItem>
        Items: 'TItem array
    }

type Msg =
    | HighlightPreviousItem
    | HighlightNextItem

let init itemPresenter items =
    {
        HighlightedIndex = 0
        ItemPresenter = itemPresenter
        Items = items
    }

let update msg model =
    match msg with
    | HighlightPreviousItem ->
        { model with HighlightedIndex = Math.Max(0, model.HighlightedIndex - 1) }
    | HighlightNextItem ->
        { model with HighlightedIndex = Math.Min(model.Items.Length - 1, model.HighlightedIndex + 1) }

let view uiContext model =
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

        UI.setCursorPosition uiContext 0 index
        UI.drawFullLine uiContext colors label

    let areaHeight = UIContext.getAreaHeight uiContext

    model.Items
    |> Seq.take areaHeight
    |> Seq.iteri drawItem
