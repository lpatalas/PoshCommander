using System.IO;
using System.Linq;
using PoshCommander.UI;

namespace PoshCommander.Commands
{
    public class CreateDirectoryCommand
    {
        private static readonly char[] invalidFileNameChars
            = Path.GetInvalidFileNameChars();

        private readonly IInputReader inputReader;

        public CreateDirectoryCommand(IInputReader inputReader)
        {
            this.inputReader = inputReader;
        }

        public void Execute(IFileSystemPane pane)
        {
            var folderName = inputReader.ReadInput(
                "Name",
                pane.View,
                IsValidPathChar);

            if (folderName.HasValue)
                pane.CreateDirectory(folderName.Value);
        }

        private static bool IsValidPathChar(char c)
            => !invalidFileNameChars.Contains(c);
    }
}
