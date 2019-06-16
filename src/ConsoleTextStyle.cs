namespace PoshCommander
{
    public struct ConsoleTextStyle
    {
        public RgbColor BackgroundColor { get; }
        public RgbColor ForegroundColor { get; }

        public ConsoleTextStyle(
            RgbColor backgroundColor,
            RgbColor foregroundColor)
        {
            this.BackgroundColor = backgroundColor;
            this.ForegroundColor = foregroundColor;
        }

        public string ApplyTo(string input)
        {
            var fgCode = AnsiEscapeCodes.ForegroundColor(ForegroundColor);
            var bgCode = AnsiEscapeCodes.BackgroundColor(BackgroundColor);
            var reset = AnsiEscapeCodes.Reset;

            return $"{bgCode}{fgCode}{input}{reset}";
        }
    }
}
