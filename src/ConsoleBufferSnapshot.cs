using System;
using System.Diagnostics;
using System.Management.Automation.Host;

namespace PoshCommander
{
    public class ConsoleBufferSnapshot : IDisposable
    {
        private readonly Coordinates ZeroCoordinates = new Coordinates(0, 0);

        private readonly Size bufferSize;
        private readonly ConsoleColor originalBackgroundColor;
        private readonly BufferCell[,] originalBuffer;
        private readonly Coordinates originalCursorPosition;
        private readonly int originalCursorSize;
        private readonly ConsoleColor originalForegroundColor;
        private readonly PSHostRawUserInterface rawUI;

        public ConsoleBufferSnapshot(PSHostRawUserInterface rawUI)
        {
            this.bufferSize = rawUI.BufferSize;
            this.originalBackgroundColor = rawUI.BackgroundColor;
            this.originalCursorPosition = rawUI.CursorPosition;
            this.originalCursorSize = rawUI.CursorSize;
            this.originalForegroundColor = rawUI.ForegroundColor;
            this.rawUI = rawUI;

            var bufferRect = new Rectangle(0, 0, bufferSize.Width - 1, bufferSize.Height - 1);
            this.originalBuffer = rawUI.GetBufferContents(bufferRect);

            var windowSize = rawUI.WindowSize;
            rawUI.WindowPosition = new Coordinates(0, bufferSize.Height - windowSize.Height);
            rawUI.CursorPosition = new Coordinates(0, bufferSize.Height - windowSize.Height);
            rawUI.CursorSize = 0;
        }

        public void Dispose()
        {
            rawUI.SetBufferContents(ZeroCoordinates, originalBuffer);
            rawUI.BackgroundColor = originalBackgroundColor;
            rawUI.CursorPosition = originalCursorPosition;
            rawUI.CursorSize = originalCursorSize;
            rawUI.ForegroundColor = originalForegroundColor;
        }
    }
}
