using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace PoshCommander
{
    public class Theme
    {
        public RgbColor IconFolderForeground { get; set; }
        public RgbColor ItemNormalForeground { get; set; }
        public RgbColor RowEvenBackground { get; set; }
        public RgbColor RowHightlighedBackground { get; set; }
        public RgbColor RowOddBackground { get; set; }
        public RgbColor SeparatorBackground { get; set; }
        public RgbColor SeparatorForeground { get; set; }
        public RgbColor StatusBarBackground { get; set; }
        public RgbColor StatusBarForeground { get; set; }
        public RgbColor TitleBarActiveBackground { get; set; }
        public RgbColor TitleBarActiveForeground { get; set; }
        public RgbColor TitleBarInactiveBackground { get; set; }
        public RgbColor TitleBarInactiveForeground { get; set; }

        public static readonly Theme Default = new Theme
        {
            IconFolderForeground = new RgbColor(255, 192, 64),
            ItemNormalForeground = new RgbColor(255, 255, 255),
            RowEvenBackground = new RgbColor(29, 40, 61),
            RowHightlighedBackground = new RgbColor(26, 64, 105),
            RowOddBackground = new RgbColor(33, 45, 67),
            SeparatorBackground = new RgbColor(29, 40, 61),
            SeparatorForeground = new RgbColor(90, 123, 181),
            StatusBarBackground = new RgbColor(80, 80, 80),
            StatusBarForeground = new RgbColor(200, 200, 200),
            TitleBarActiveBackground = new RgbColor(90, 123, 181),
            TitleBarActiveForeground = new RgbColor(255, 255, 255),
            TitleBarInactiveBackground = new RgbColor(32, 32, 32),
            TitleBarInactiveForeground = new RgbColor(96, 96, 96),
        };

        public string GetIcon(FileSystemItem item)
        {
            switch (item.Kind)
            {
                case FileSystemItemKind.Directory:
                    return $"{AnsiEscapeCodes.ForegroundColor(IconFolderForeground)}\uF74A";
                case FileSystemItemKind.File:
                    return GetFileIcon(item.Name);
                case FileSystemItemKind.ParentDirectory:
                    return $"{AnsiEscapeCodes.ForegroundColor(IconFolderForeground)}\uF758";
                case FileSystemItemKind.SymbolicLink:
                    return "\uF751";
                default:
                    throw new InvalidEnumValueException<FileSystemItemKind>(item.Kind);
            }
        }

        private string GetFileIcon(string fileName)
        {
            return fileStyles
                .First(style => style.Matches(fileName))
                .Icon;
        }

        private static readonly List<FileStyle> fileStyles = new List<FileStyle>
        {
            new FileStyle(@"\.html?$", '\uF13B', new RgbColor(255, 192, 192)),
            new FileStyle(@"\.cs$", '\uF81A', new RgbColor(192, 255, 192)),
            new FileStyle(@"\.jsx?$", '\uE74E', new RgbColor(255, 255, 192)),
            new FileStyle(@"\.tsx?$", '\uFBE4', new RgbColor(192, 192, 255)),
            new FileStyle(@"\.csproj$", '\uE70C', new RgbColor(192, 255, 192)),
            new FileStyle(@"\.sln$", '\uE70C', new RgbColor(192, 255, 255)),
            new FileStyle(@"\.json$", '\uE60B', new RgbColor(255, 255, 192)),
            new FileStyle(@"\.(cfg|config|ini|settings)$", '\uE615', new RgbColor(255, 255, 192)),
            new FileStyle(@"\.(bmp|gif|jpe?g|png|svg|tiff?)$", '\uE60D', new RgbColor(255, 192, 255)),
            new FileStyle(@"\.(bat|cmd|ps1|psd1|psm1)?$", '\uF120', new RgbColor(192, 128, 255)),
            new FileStyle(@"\.(md|rst|txt)$", '\uF72D'),
            new FileStyle(@"^\.git(attributes|config|ignore)$", '\uF1D3', new RgbColor(255, 192, 128)),
            new FileStyle(@".", '\uF713')
        };

        private class FileStyle
        {
            private readonly Regex fileNameRegex;

            public string Icon { get; }

            public FileStyle(string fileNamePattern, char iconGlyph, RgbColor? color = null)
            {
                fileNameRegex = new Regex(fileNamePattern, RegexOptions.Singleline);
                Icon = FormatIcon(iconGlyph, color);
            }

            public bool Matches(string fileName)
                => fileNameRegex.IsMatch(fileName);

            private static string FormatIcon(char icon, RgbColor? color)
            {
                var colorCode = color.HasValue
                    ? AnsiEscapeCodes.ForegroundColor(color.Value)
                    : string.Empty;

                return $"{colorCode}{icon}";
            }
        }
    }
}
