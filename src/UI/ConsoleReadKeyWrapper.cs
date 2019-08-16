using System;

namespace PoshCommander.UI
{
    public class ConsoleReadKeyWrapper : IConsoleReadKeyWrapper
    {
        public ConsoleKeyInfo ReadKey()
            => Console.ReadKey(intercept: true);
    }
}
