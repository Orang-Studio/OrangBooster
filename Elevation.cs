using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
namespace OrangBooster
{
    public static class Elevation
    {
        public const string ElevatedArg = "--elevated";

        public static bool IsElevated()
        {
            using var id = WindowsIdentity.GetCurrent();
            return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
        }
        public static bool RelaunchElevated(string? extraArgs = null)
        {
            string? exe = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName;
            if (string.IsNullOrEmpty(exe)) return false;
            try
            {
                string args = string.IsNullOrWhiteSpace(extraArgs) ? ElevatedArg : $"{ElevatedArg} {extraArgs}";
                Process.Start(new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = args,
                    UseShellExecute = true,
                    Verb = "runas",
                    WorkingDirectory = Path.GetDirectoryName(exe) ?? string.Empty,
                });
                return true;
            }
            catch
            {
                return false;
}   }   }   }