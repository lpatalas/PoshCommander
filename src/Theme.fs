namespace PoshCommander

open System.Text.RegularExpressions

type RgbColor = RgbColor of int * int * int

module Theme =
    let IconFolderForeground = RgbColor (255, 192, 64)
    let ItemNormalForeground = RgbColor (255, 255, 255)
    let ItemSelectedForeground = RgbColor (255, 128, 64)
    let RowEvenBackground = RgbColor (29, 40, 61)
    let RowHightlighedBackground = RgbColor (26, 64, 105)
    let RowOddBackground = RgbColor (33, 45, 67)
    let SeparatorBackground = RgbColor (29, 40, 61)
    let SeparatorForeground = RgbColor (90, 123, 181)
    let StatusBarBackground = RgbColor (80, 80, 80)
    let StatusBarForeground = RgbColor (200, 200, 200)
    let TitleBarActiveBackground = RgbColor (90, 123, 181)
    let TitleBarActiveForeground = RgbColor (255, 255, 255)
    let TitleBarInactiveBackground = RgbColor (32, 32, 32)
    let TitleBarInactiveForeground = RgbColor (96, 96, 96)

    type Icon = {
        Color: RgbColor
        Glyph: char
        }

    type IconPattern = {
        Icon: Icon
        Pattern: Regex
        }

    let defaultFilePatterns =
        let createStyle (rgb, glyph, pattern) =
            {
                Icon = { Color = RgbColor rgb; Glyph = glyph }
                Pattern = new Regex(pattern, RegexOptions.IgnoreCase)
            }

        [|
            ( (255, 192, 192), '\uF13B', @"\.html?$" )
            ( (192, 255, 192), '\uF81A', @"\.cs$" )
            ( (255, 255, 192), '\uE74E', @"\.jsx?$" )
            ( (192, 192, 255), '\uE628', @"\.tsx?$" )
            ( (192, 255, 192), '\uE70C', @"\.csproj$" )
            ( (192, 128, 255), '\uE70C', @"\.sln$" )
            ( (255, 255, 192), '\uE60B', @"\.(json|yaml)$" )
            ( (255, 128, 192), '\uF121', @"\.xml$" )
            ( (255, 255, 255), '\uF488', @"\.exe$" )
            ( (255, 255, 128), '\uF471', @"\.(bin|dll|pdb)$" )
            ( (255, 255, 128), '\uF410', @"\.(7z|gz|rar|zip)" )
            ( (255, 255, 192), '\uE615', @"\.(cfg|config|ini|settings)$" )
            ( (255, 192, 255), '\uE60D', @"\.(bmp|gif|jpe?g|png|svg|tiff?)$" )
            ( (192, 128, 255), '\uF120', @"\.(bat|cmd|ps1|psd1|psm1)?$" )
            ( (192, 192, 255), '\uE73E', @"\.(md|markdown)$" )
            ( (255, 128,  64), '\uE702', @"^\.git(attributes|config|ignore)?$" )
            ( (255, 255, 255), '\uF713', @"." )
        |]
        |> Array.map createStyle

    let directoryIcon = { Color = IconFolderForeground; Glyph = '\uF74A' }
    let defaultFileIcon = { Color = RgbColor (255, 255, 255); Glyph = '\uF713' }

    let getFileIcon defaultIcon (iconPatterns: IconPattern[]) fileName =
        iconPatterns
        |> Array.tryFind (fun { Pattern = pattern } -> pattern.IsMatch(fileName))
        |> Option.map (fun { Icon = icon } -> icon)
        |> Option.defaultValue defaultIcon
