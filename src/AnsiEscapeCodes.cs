using System.Text.RegularExpressions;

namespace PoshCommander
{
    public static class AnsiEscapeCodes
    {
        private static readonly Regex escapeCodeRegex = new Regex("\u001b[^m]+m");
        private const char esc = (char)0x1b;

        public static readonly string Reset = $"{esc}[0m";

        public static string BackgroundColor(RgbColor color)
            => TrueColor(color, isForeground: false);

        public static string ForegroundColor(RgbColor color)
            => TrueColor(color, isForeground: true);

        public static string StripEscapeCodes(string text)
            => escapeCodeRegex.Replace(text, string.Empty);

        private static string TrueColor(RgbColor color, bool isForeground)
        {
            var fgbgCode = isForeground ? 38 : 48;
            return $"{AnsiEscapeCodes.esc}[{fgbgCode};2;{color.R};{color.G};{color.B}m";
        }
    }
}
