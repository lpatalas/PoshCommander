﻿module PoshCommander.Commands.HighlightingCommands

open PoshCommander

let clamp min max value =
    if value < min then
        min
    else if value > max then
        max
    else
        value

let setHighlightedIndex adjuster (pane: Pane) =
    let newIndex =
        pane
        |> adjuster
        |> clamp 0 (pane.CurrentDirectory.Items.Count - 1)

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

let highlightFirstItem = setHighlightedIndex (fun _ -> 0)
let highlightLastItem = setHighlightedIndex (fun pane -> pane.CurrentDirectory.Items.Count - 1)
let highlightNextItem = setHighlightedIndex (fun pane -> pane.HighlightedIndex + 1)
let highlightPreviousItem = setHighlightedIndex (fun pane -> pane.HighlightedIndex - 1)
let highlightItemOnePageBefore = setHighlightedIndex (fun pane -> pane.HighlightedIndex - pane.RowCount + 1)
let highlightItemOnePageAfter = setHighlightedIndex (fun pane -> pane.HighlightedIndex + pane.RowCount - 1)
