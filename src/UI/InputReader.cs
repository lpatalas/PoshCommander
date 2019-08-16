using System;

namespace PoshCommander.UI
{
    public class InputReader : IInputReader
    {
        private readonly IConsoleReadKeyWrapper readKeyWrapper;

        public InputReader(IConsoleReadKeyWrapper readKeyWrapper)
        {
            this.readKeyWrapper = readKeyWrapper;
        }

        public Option<string> ReadInput(
            string prompt,
            IPaneView view,
            Predicate<char> isCharacterValid)
        {
            var inputText = string.Empty;
            var done = false;
            var originalStatusText = view.StatusText;

            while (!done)
            {
                DrawTextInput(prompt, inputText, view);

                var keyInfo = readKeyWrapper.ReadKey();

                if (keyInfo.Key == ConsoleKey.Escape)
                    return Option.None;
                else if (keyInfo.Key == ConsoleKey.Enter)
                    done = true;
                else if (keyInfo.Key == ConsoleKey.Backspace && inputText.Length > 0)
                    inputText = inputText.Substring(0, inputText.Length - 1);
                else if (isCharacterValid(keyInfo.KeyChar))
                    inputText += keyInfo.KeyChar;
            }

            view.StatusText = originalStatusText;
            view.DrawStatusBar();

            return Option.Some(inputText);
        }

        private void DrawTextInput(string prompt, string input, IPaneView view)
        {
            view.StatusText = $"{prompt}: {input}";
            view.DrawStatusBar();
        }
    }
}
