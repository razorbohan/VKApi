using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using File = System.IO.File;

namespace VkApi.Utility
{
    internal static class Logger
    {
        public static string FilePath { get; set; }
        public static FrameworkElement ControlLog { get; set; }

        static Logger()
        {
            FilePath = "Logs.txt";
        }

        public static void Log(string logMessage)
        {
            try
            {
                lock (logMessage)
                {
                    var message = !logMessage.StartsWith(new string('-', 80))
                        ? $"{DateTime.Now:dd.MM.yyyy HH:mm:ss} {logMessage}\r\n"
                        : $"{logMessage}\r\n";

                    File.AppendAllText(FilePath, message);
                    ControlLog?.Dispatcher.Invoke(() =>
                    {
                        ((RichTextBox)ControlLog).AppendText(message);
                    });
                }
            }
            catch (Exception ex)
            {
                ErrorLog(ex);
            }
        }

        public static void ErrorLog(Exception ex)
        {
            var stackTrace = new StackTrace(ex, true);
            var frame = stackTrace.GetFrame(stackTrace.FrameCount - 1);
            var lineNumber = frame?.GetFileLineNumber();
            var source = frame?.GetMethod().DeclaringType;

            Debug.WriteLine(ex.Message);
            Log($"Error in {source?.ToString() ?? "null"} on {lineNumber?.ToString() ?? "null"}: {ex.Message}");

            if (ex.InnerException != null)
                ErrorLog(ex.InnerException);
        }
    }
}