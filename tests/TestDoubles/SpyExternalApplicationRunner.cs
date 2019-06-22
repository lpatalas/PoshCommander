using System.Collections.Generic;

namespace PoshCommander.Tests.TestDoubles
{
    public class SpyExternalApplicationRunner : IExternalApplicationRunner
    {
        private readonly List<string> executedFileList = new List<string>();
        public IReadOnlyList<string> ExecutedFiles => executedFileList;

        public void RunAssociatedApplication(string filePath)
        {
            executedFileList.Add(filePath);
        }

        private readonly List<string> editedFileList = new List<string>();
        public IReadOnlyList<string> EditedFiles => editedFileList;

        public void RunEditor(string filePath)
        {
            editedFileList.Add(filePath);
        }

        private readonly List<string> viewedFileList = new List<string>();
        public IReadOnlyList<string> ViewedFiles => viewedFileList;

        public void RunViewer(string filePath)
        {
            viewedFileList.Add(filePath);
        }
    }
}
