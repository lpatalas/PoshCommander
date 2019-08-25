module PoshCommander.Commands.HighlightingCommands

open PoshCommander

let clamp min max value =
    if value < min then
        min
    else if value > max then
        max
    else
        value

let setHighlightedIndex adjuster (paneState: PaneState) =
    let newIndex =
        paneState
        |> adjuster
        |> clamp 0 (paneState.Items.Length - 1)

    { paneState with HighlightedIndex = newIndex }

let highlightFirstItem = setHighlightedIndex (fun _ -> 0)
let highlightLastItem = setHighlightedIndex (fun pane -> pane.Items.Length - 1)
let highlightNextItem = setHighlightedIndex (fun pane -> pane.HighlightedIndex + 1)
let highlightPreviousItem = setHighlightedIndex (fun pane -> pane.HighlightedIndex - 1)
