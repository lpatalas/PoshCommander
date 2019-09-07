﻿module PoshCommander.Tests.Theme

open NUnit.Framework
open PoshCommander.Theme
open PoshCommander.Colors
open System.Text.RegularExpressions
open Swensen.Unquote

let createWhiteIcon glyph =
    { Color = RgbColor (255, 255, 255); Glyph = glyph }

let createPattern glyphIndex regexPattern =
    {
        Icon = createWhiteIcon (char (65 + glyphIndex))
        Pattern = new Regex(regexPattern)
    }

let createPatterns patterns =
    patterns
    |> Seq.mapi createPattern
    |> Seq.toArray

let defaultIcon = createWhiteIcon '0'

module getFileIcon =
    [<Test>]
    let ``should return defaultIcon when file name does not match any pattern``() =
        let patterns = createPatterns [ @"\.txt" ]
        let result = getFileIcon defaultIcon patterns "file.bmp"
        test <@ result = defaultIcon @>

    [<Test>]
    let ``should return first match if file name matches multple patterns``() =
        let patterns = createPatterns [ @"\.tx"; @"\.txt" ]
        let result = getFileIcon defaultIcon patterns "file.txt"
        test <@ result = patterns.[0].Icon @>
    
    [<Test>]
    let ``should skip patterns which does not match file name``() =
        let patterns = createPatterns [ @"\.bmp"; @"\.png"; @"\.txt" ]
        let result = getFileIcon defaultIcon patterns "file.txt"
        test <@ result = patterns.[2].Icon @>
