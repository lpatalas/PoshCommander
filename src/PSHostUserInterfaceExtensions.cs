using System.Management.Automation.Host;

namespace PoshCommander
{
    public static class PSHostUserInterfaceExtensions
    {
        public static void WriteAt(
            this PSHostUserInterface ui,
            string text,
            Coordinates coordinates,
            ConsoleTextStyle textStyle)
        {
            ui.RawUI.CursorPosition = new Coordinates(
                coordinates.X,
                1000 - ui.RawUI.WindowSize.Height + coordinates.Y);
            var styledText = textStyle.ApplyTo(text);
            ui.Write(styledText);
        }

        public static void WriteBlockAt(
            this PSHostUserInterface ui,
            string text,
            Coordinates coordinates,
            int blockWidth,
            ConsoleTextStyle textStyle)
        {
            if (text.Length > blockWidth)
                text = text.Substring(0, blockWidth - 3) + "...";

            var paddedText = text + new string(' ', blockWidth - text.Length);
            ui.WriteAt(paddedText, coordinates, textStyle);
        }
    }
}
