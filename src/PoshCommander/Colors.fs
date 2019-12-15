module PoshCommander.Colors

type ColorLayer = Background | Foreground
type RgbColor = RgbColor of int * int * int

let ansiEsc = '\x1b'
let ansiResetCode = "\x1b[0m"

let toAnsiColorCode layer (RgbColor (r, g, b)) =
    let layerCode =
        match layer with
        | Background -> 48
        | Foreground -> 38

    sprintf "%c[%i;2;%i;%i;%im" ansiEsc layerCode r g b

let toAnsiBgColorCode = toAnsiColorCode Background
let toAnsiFgColorCode = toAnsiColorCode Foreground
