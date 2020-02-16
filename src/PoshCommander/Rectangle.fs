namespace PoshCommander
open System.Management.Automation.Host

type Rect = {
    Left: int
    Top: int
    Width: int
    Height: int
    }

module Rect =
    let fromSize (size: Size) =
        { Left = 0; Top = 0; Width = size.Width; Height = size.Height }

    let splitHorizontally rect =
        let halfWidth = rect.Width / 2
        let leftRect = { Left = rect.Left; Top = rect.Top; Width = halfWidth; Height = rect.Height }
        let rightRect = { Left = rect.Left + halfWidth + 1; Top = rect.Top; Width = rect.Width - halfWidth - 2; Height = rect.Height }
        (leftRect, rightRect)
