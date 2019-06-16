namespace PoshCommander
{
    public class Theme
    {
        public RgbColor IconFolderForeground { get; set; }
        public RgbColor ItemNormalForeground { get; set; }
        public RgbColor RowEvenBackground { get; set; }
        public RgbColor RowHightlighedBackground { get; set; }
        public RgbColor RowOddBackground { get; set; }
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
            StatusBarBackground = new RgbColor(80, 80, 80),
            StatusBarForeground = new RgbColor(200, 200, 200),
            TitleBarActiveBackground = new RgbColor(90, 123, 181),
            TitleBarActiveForeground = new RgbColor(255, 255, 255),
            TitleBarInactiveBackground = new RgbColor(32, 32, 32),
            TitleBarInactiveForeground = new RgbColor(96, 96, 96),
        };
    }
}
