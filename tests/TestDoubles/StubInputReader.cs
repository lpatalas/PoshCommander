using System;
using PoshCommander.UI;

namespace PoshCommander.Tests.TestDoubles
{
    public class StubInputReader : IInputReader
    {
        private readonly Option<string> result;

        public StubInputReader(string result)
        {
            this.result = Option.Some(result);
        }

        public StubInputReader(None none)
        {
            this.result = none;
        }

        public Option<string> ReadInput(
            string prompt,
            IPaneView view,
            Predicate<char> characterValidator)
            => result;
    }
}
