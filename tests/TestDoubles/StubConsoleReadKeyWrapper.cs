using System;
using System.Collections.Generic;
using System.Linq;
using PoshCommander.UI;

namespace PoshCommander.Tests.TestDoubles
{
    public class StubConsoleReadKeyWrapper : IConsoleReadKeyWrapper
    {
        private readonly Queue<ConsoleKeyInfo> keyInfoQueue;

        public StubConsoleReadKeyWrapper(string input)
        {
            keyInfoQueue = new Queue<ConsoleKeyInfo>(
                StringToKeyInfo(input)
                    .Concat(new[] { new ConsoleKeyInfo('\r', ConsoleKey.Enter, false, false, false) }));
        }

        public StubConsoleReadKeyWrapper(params ConsoleKey[] keys)
        {
            var keyInfos = keys
                .Select(key => key.ToKeyInfo())
                .ToList();

            keyInfoQueue = new Queue<ConsoleKeyInfo>(keyInfos);
        }

        public ConsoleKeyInfo ReadKey()
            => keyInfoQueue.Dequeue();

        private static ConsoleKeyInfo[] StringToKeyInfo(string input)
        {
            return input.Select(CharToKeyInfo).ToArray();
        }

        private static ConsoleKeyInfo CharToKeyInfo(char c)
        {
            var upperChar = char.ToUpper(c);
            if (upperChar >= 'A' && upperChar <= 'Z')
            {
                return new ConsoleKeyInfo(
                    c,
                    (ConsoleKey)upperChar,
                    shift: char.IsUpper(c),
                    alt: false,
                    control: false);
            }
            else
            {
                return new ConsoleKeyInfo(c, (ConsoleKey)c, false, false, false);
            }
        }
    }
}
