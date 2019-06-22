using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;

namespace PoshCommander
{
    public class ExternalApplicationRunner : IExternalApplicationRunner
    {
        public void Run(string filePath)
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
    }
}
