using System.Collections.Generic;

namespace PoshCommander.Tests
{
    public class SpyExternalApplicationRunner : IExternalApplicationRunner
    {
        private readonly List<string> executedFilesList = new List<string>();
        public IReadOnlyList<string> ExecutedFiles => executedFilesList;

        public void Run(string filePath)
        {
            executedFilesList.Add(filePath);
        }
    }
}
