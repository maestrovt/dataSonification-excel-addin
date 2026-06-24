using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;

namespace DataSonificationLib
{
    // Stage 6 instrumentation. Writes a timestamped, thread-tagged log to
    // %LOCALAPPDATA%\dataSonification\sonify.log so the XLL (which has no
    // console) leaves a trail when running inside Excel. Used to identify
    // who emits the STOP control message that triggers DATA_SOURCE_NULL_ERROR.
    internal static class SonifyLog
    {
        private static readonly object writeLock = new object();
        private static readonly string logPath;
        private static readonly bool enabled;

        static SonifyLog()
        {
            try
            {
                string dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "dataSonification");
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                logPath = Path.Combine(dir, "sonify.log");
                enabled = true;
                int pid = System.Diagnostics.Process.GetCurrentProcess().Id;
                AppendLine("===== Logger initialized. PID=" + pid + " =====");
            }
            catch
            {
                enabled = false;
            }
        }

        public static void Write(string message, [CallerMemberName] string caller = null)
        {
            if (!enabled) return;
            try
            {
                AppendLine(string.Format(
                    "[T{0,3}] {1}: {2}",
                    Thread.CurrentThread.ManagedThreadId,
                    caller ?? "?",
                    message));
            }
            catch { /* never let logging crash callers */ }
        }

        public static void WriteWithStack(string message, [CallerMemberName] string caller = null)
        {
            if (!enabled) return;
            try
            {
                string stack = new System.Diagnostics.StackTrace(1, false).ToString();
                AppendLine(string.Format(
                    "[T{0,3}] {1}: {2}\n  stack:\n{3}",
                    Thread.CurrentThread.ManagedThreadId,
                    caller ?? "?",
                    message,
                    stack));
            }
            catch { }
        }

        private static void AppendLine(string body)
        {
            lock (writeLock)
            {
                File.AppendAllText(
                    logPath,
                    string.Format("{0:yyyy-MM-dd HH:mm:ss.fff} {1}{2}",
                        DateTime.Now, body, Environment.NewLine));
            }
        }
    }
}
