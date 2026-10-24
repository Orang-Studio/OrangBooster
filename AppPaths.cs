using System;
using System.IO;

namespace OrangBooster
{
    public static class AppPaths
    {
        public static readonly string Root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "OrangStudio", "OrangBooster");

        public static readonly string LogsDir = Path.Combine(Root, "logs");
        public static readonly string CacheDir = Path.Combine(Root, "cache");
        public static readonly string TempDir = Path.Combine(Root, "temp");

        static AppPaths()
        {
            try { Directory.CreateDirectory(LogsDir); } catch { }
            try { Directory.CreateDirectory(CacheDir); } catch { }
            try { Directory.CreateDirectory(TempDir); } catch { }
            Environment.SetEnvironmentVariable("OB_TEMP", TempDir);
        }
    }
}
