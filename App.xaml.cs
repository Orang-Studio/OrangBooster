using Microsoft.UI.Xaml;
using System;
using System.Linq;
using System.Threading.Tasks;
namespace OrangBooster
{
    public partial class App : Application
    {
        public static bool LaunchedElevated { get; private set; }
        private Window? _window;
        public App()
        {
            InitializeComponent();
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Logger.Info($"App() ctor on PID {Environment.ProcessId}, .NET {Environment.Version}, CWD {Environment.CurrentDirectory}");
        }
        private void CurrentDomain_UnhandledException(object sender, System.UnhandledExceptionEventArgs e)
        {
            Logger.Error("UNHANDLED EXCEPTION", e.ExceptionObject as Exception);
            Logger.Flush();
        }
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            try
            {
                var cliArgs = Environment.GetCommandLineArgs().Skip(1)
                    .Where(a => a != Elevation.ElevatedArg).ToArray();
                if (CliRunner.IsCliInvocation(cliArgs))
                {
                    if (CliRunner.WantsElevation(cliArgs) && !Elevation.IsElevated())
                    {
                        Elevation.RelaunchElevated(string.Join(" ", cliArgs));
                        Logger.Flush();
                        Exit();
                        return;
                    }
                    int code = Task.Run(() => CliRunner.RunAsync(cliArgs)).GetAwaiter().GetResult();
                    Logger.Flush();
                    Environment.Exit(code);
                    return;
                }
                bool elevated = Elevation.IsElevated();
                Logger.Info($"OnLaunched - elevated={elevated}, args={args.Arguments}");
                if (!elevated)
                {
                    Logger.Info("Not elevated, requesting relaunch with runas");
                    if (Elevation.RelaunchElevated())
                    {
                        Logger.Info("Elevated relaunch fired; exiting this process");
                        Logger.Flush();
                        Exit();
                        return;
                    }
                    Logger.Warn("Failed to relaunch elevated; continuing as user");
                }
                LaunchedElevated = Elevation.IsElevated();
                _ = ScriptRunner.PrefetchAsync();
                _window = new MainWindow();
                _window.Activate();
                Logger.Info("MainWindow activated");
            }
            catch (Exception ex)
            {
                Logger.Error("OnLaunched failed", ex);
                throw;
}   }   }   }