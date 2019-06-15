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
            ui.RawUI.CursorPosition = coordinates;
            ui.Write(
                textStyle.ForegroundColor,
                textStyle.BackgroundColor,
                text);
        }
    }
}
