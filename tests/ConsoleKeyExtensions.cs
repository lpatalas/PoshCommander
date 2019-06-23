using System;

namespace PoshCommander.Tests
{
    public static class ConsoleKeyExtensions
    {
        public static ConsoleKeyInfo ToKeyInfo(
            this ConsoleKey consoleKey,
            bool control = false,
            bool shift = false)
            => new ConsoleKeyInfo((char)consoleKey, consoleKey, shift, false, control);
    }
}
