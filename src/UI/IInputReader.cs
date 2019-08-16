using System;

namespace PoshCommander.UI
{
    public interface IInputReader
    {
        Option<string> ReadInput(
            string prompt,
            IPaneView view,
            Predicate<char> characterValidator);
    }
}
