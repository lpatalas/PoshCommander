using System;

namespace PoshCommander
{
    public struct ConsoleTextStyle
    {
        public ConsoleColor BackgroundColor { get; }
        public ConsoleColor ForegroundColor { get; }

        public ConsoleTextStyle(
            ConsoleColor backgroundColor,
            ConsoleColor foregroundColor)
        {
            this.BackgroundColor = backgroundColor;
            this.ForegroundColor = foregroundColor;
        }
    }
}
