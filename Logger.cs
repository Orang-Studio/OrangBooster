using System;
using System.Diagnostics;
using System.IO;
namespace OrangBooster
{
    public enum LogLevel { Debug, Info, Warn, Error }
    public static class Logger
    {
        public static readonly string LogDir = AppPaths.LogsDir;
        public static readonly string LogFile;
        public static bool Enabled { get; set; } = true;
        public static LogLevel MinLevel { get; set; } = LogLevel.Debug;
        static readonly object _gate = new();
        static StreamWriter? _writer;
        public delegate void LogSink(LogLevel level, string line);
        public static event LogSink? OnLine;
        static Logger()
        {
            try { Directory.CreateDirectory(LogDir); } catch { }
            string stamp = DateTime.Now.ToString("yyyy-MM-dd-HHmmss");
            LogFile = Path.Combine(LogDir, $"orangbooster-{stamp}.log");
            try
            {
                _writer = new StreamWriter(new FileStream(LogFile, FileMode.Create, FileAccess.Write, FileShare.Read))
                {
                    AutoFlush = true,
                };
                _writer.WriteLine($"=== OrangBooster session opened {DateTime.Now:o} (PID {Environment.ProcessId}) ===");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Logger init: {ex.Message}");
            }
            PruneOld();
        }
        public static void Debug(string msg) => Write(LogLevel.Debug, msg);
        public static void Info(string msg) => Write(LogLevel.Info, msg);
        public static void Warn(string msg) => Write(LogLevel.Warn, msg);
        public static void Error(string msg, Exception? ex = null)
        {
            if (ex == null) Write(LogLevel.Error, msg);
            else Write(LogLevel.Error, $"{msg} :: {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
        }
        public static void Write(LogLevel level, string msg)
        {
            if (!Enabled) return;
            if (level < MinLevel) return;
            string line = $"{DateTime.Now:HH:mm:ss.fff} [{Level3(level)}] [T{Environment.CurrentManagedThreadId,-3}] {msg}";
            try
            {
                lock (_gate) _writer?.WriteLine(line);
            }
            catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"Log write: {ex.Message}"); }
            try { OnLine?.Invoke(level, line); } catch { }
            System.Diagnostics.Debug.WriteLine(line);
        }
        public static void Flush()
        {
            try { lock (_gate) _writer?.Flush(); } catch { }
        }
        public static void OpenFolder()
        {
            try { Process.Start(new ProcessStartInfo { FileName = "explorer.exe", Arguments = $"\"{LogDir}\"", UseShellExecute = true }); }
            catch (Exception ex) { Error("OpenFolder failed", ex); }
        }
        public static void OpenCurrent()
        {
            try { Process.Start(new ProcessStartInfo { FileName = LogFile, UseShellExecute = true }); }
            catch (Exception ex) { Error("OpenCurrent failed", ex); }
        }
        static void PruneOld()
        {
            try
            {
                var files = new DirectoryInfo(LogDir).GetFiles("orangbooster-*.log");
                if (files.Length <= 20) return;
                Array.Sort(files, (a, b) => a.LastWriteTimeUtc.CompareTo(b.LastWriteTimeUtc));
                for (int i = 0; i < files.Length - 20; i++)
                {
                    try { files[i].Delete(); } catch { }
                }
            }
            catch { }
        }
        static string Level3(LogLevel l) => l switch
        {
            LogLevel.Debug => "DBG",
            LogLevel.Info => "INF",
            LogLevel.Warn => "WRN",
            _ => "ERR",
        };
}   }