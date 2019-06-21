using System;

namespace PoshCommander.Tests
{
    public static class ConsoleKeyExtensions
    {
        public static ConsoleKeyInfo ToKeyInfo(this ConsoleKey consoleKey)
            => new ConsoleKeyInfo(' ', consoleKey, false, false, false);
    }
}
