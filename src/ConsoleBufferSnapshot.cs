using System;
using System.Management.Automation.Host;

namespace PoshCommander
{
    public class ConsoleBufferSnapshot : IDisposable
    {
        private readonly Coordinates ZeroCoordinates = new Coordinates(0, 0);

        private readonly Size bufferSize;
        private readonly BufferCell[,] originalBuffer;
        private readonly Coordinates originalCursorPosition;
        private readonly int originalCursorSize;
        private readonly PSHostRawUserInterface rawUI;

        public ConsoleBufferSnapshot(PSHostRawUserInterface rawUI)
        {
            this.bufferSize = rawUI.BufferSize;
            this.originalCursorPosition = rawUI.CursorPosition;
            this.originalCursorSize = rawUI.CursorSize;
            this.rawUI = rawUI;

            var bufferRect = new Rectangle(0, 0, bufferSize.Width - 1, bufferSize.Height - 1);
            this.originalBuffer = rawUI.GetBufferContents(bufferRect);

            var bufferCell = new BufferCell(' ', ConsoleColor.Gray, ConsoleColor.Black, BufferCellType.Complete);
            var newBuffer = rawUI.NewBufferCellArray(bufferSize, bufferCell);
            rawUI.SetBufferContents(ZeroCoordinates, newBuffer);
            rawUI.CursorPosition = ZeroCoordinates;
            rawUI.CursorSize = 0;
        }

        public void Dispose()
        {
            rawUI.SetBufferContents(ZeroCoordinates, originalBuffer);
            rawUI.CursorPosition = originalCursorPosition;
            rawUI.CursorSize = originalCursorSize;
        }
    }
}
