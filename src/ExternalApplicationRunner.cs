using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace PoshCommander
{
    public class ExternalApplicationRunner : IExternalApplicationRunner
    {
        private readonly string editorPath;
        private readonly string viewerPath;

        public ExternalApplicationRunner(string editorPath, string viewerPath)
        {
            this.editorPath = editorPath;
            this.viewerPath = viewerPath;
        }

        public void RunAssociatedApplication(string filePath)
        {
            var startInfo = new ProcessStartInfo(filePath)
            {
                UseShellExecute = true
            };

            try
            {
                Process.Start(startInfo);
            }
            catch (Win32Exception exception)
            {
                if (exception.NativeErrorCode == NativeErrorCodes.ERROR_NO_ASSOCIATION)
                {
                    // TODO: Show message box telling user that there is no default
                    //       application associated with given file
                }
            }
        }

        public void RunEditor(string filePath)
            => RunApplicationWithArgument(editorPath, filePath);

        public void RunViewer(string filePath)
            => RunApplicationWithArgument(viewerPath, filePath);

        private static void RunApplicationWithArgument(string applicationPath, string argument)
        {
            if (applicationPath.IsNullOrEmpty())
                return;

            var startInfo = new ProcessStartInfo(applicationPath)
            {
                Arguments = argument
            };

            Process.Start(startInfo);

        }
    }
}
