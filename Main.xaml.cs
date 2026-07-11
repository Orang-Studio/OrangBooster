using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
namespace OrangBooster
{
    public sealed partial class MainWindow : Window
    {
        public ObservableCollection<AppItem> FilteredApps { get; } = new();
        public ObservableCollection<BoosterCard> BoosterCards { get; } = new();
        private readonly List<AppItem> _allApps = new();
        private readonly List<BoosterCard> _allBoosterCards = new();
        private bool _appsLoaded;
        private bool _warningShown;
        public MainWindow()
        {
            InitializeComponent();
            ApplyBackdrop();
            try
            {
                ExtendsContentIntoTitleBar = true;
                SetTitleBar(AppTitleBar);
            }
            catch (Exception ex) { Logger.Error("TitleBar setup", ex); }

            try
            {
                string icoPath = Path.Combine(AppContext.BaseDirectory, "Assets", "OrangBooster.ico");
                if (File.Exists(icoPath)) AppWindow.SetIcon(icoPath);
            }
            catch (Exception ex) { Logger.Error("Window icon setup", ex); }

            try
            {
                TitleBarLogo.Source = LoadAssetImage("StoreLogo.png");
                AboutLogo.Source = LoadAssetImage("Square150x150Logo.scale-200.png");
            }
            catch (Exception ex) { Logger.Error("Logo image setup", ex); }

            _allBoosterCards.AddRange(BoosterCatalog.Build());
            foreach (var c in _allBoosterCards) BoosterCards.Add(c);
            Logger.Info($"Loaded {BoosterCards.Count} Booster cards");
            _ = LoadAppsAsync();
            try { LogPathText.Text = Logger.LogFile; } catch { }
            Activated += MainWindow_Activated;
        }
        private async void MainWindow_Activated(object sender, WindowActivatedEventArgs args)
        {
            if (_warningShown) return;
            _warningShown = true;
            await Task.Delay(300);
            try { await ShowStartupWarningAsync(); } catch (Exception ex) { Debug.WriteLine($"Warning: {ex.Message}"); }
        }
        private static bool IsWindows11()
        {
            try
            {
                using var k = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
                if (k?.GetValue("CurrentBuildNumber") is string b && int.TryParse(b, out int build))
                    return build >= 22000;
            }
            catch { }
            return true;
        }
        private async Task ShowStartupWarningAsync()
        {
            if (!IsWindows11())
            {
                var block = new ContentDialog
                {
                    Title = "Windows 11 required",
                    Content = new TextBlock
                    {
                        TextWrapping = TextWrapping.Wrap,
                        Text = "OrangBooster only supports Windows 11. This machine is running Windows 10, so the app will now close.",
                    },
                    PrimaryButtonText = "Close",
                    DefaultButton = ContentDialogButton.Primary,
                    XamlRoot = ((FrameworkElement)Content).XamlRoot,
                };
                await block.ShowAsync();
                Application.Current.Exit();
                return;
            }

            var content = new TextBlock
            {
                TextWrapping = TextWrapping.Wrap,
                Text =
                    "OrangBooster modifies system configuration: registry keys, services, optional features, " +
                    "and may uninstall built-in apps. Changes can be hard to revert.\n\n" +
                    "Windows Defender / SmartScreen may flag this app as suspicious - that is a false positive " +
                    "caused by the registry edits and silent PowerShell invocations. No telemetry, no payload - " +
                    "source is on GitHub.\n\n" +
                    "Use at your own risk. Create a restore point before running destructive sets."
            };
            var dlg = new ContentDialog
            {
                Title = "Heads up before you boost",
                Content = content,
                PrimaryButtonText = "I understand, continue",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = ((FrameworkElement)Content).XamlRoot,
            };
            await dlg.ShowAsync();
        }

        private async Task LoadAppsAsync()
        {
            try
            {
                string path = Path.Combine(AppContext.BaseDirectory, "Assets", "applications.json");
                string json = await File.ReadAllTextAsync(path);
                var parsed = await Task.Run(() => ParseApps(json));
                DispatcherQueue.TryEnqueue(() =>
                {
                    _allApps.Clear();
                    _allApps.AddRange(parsed);
                    FilteredApps.Clear();
                    foreach (var a in _allApps) FilteredApps.Add(a);
                    _appsLoaded = true;
                });
            }
            catch (Exception ex) { Debug.WriteLine($"LoadApps: {ex.Message}"); }
        }
        private static List<AppItem> ParseApps(string json)
        {
            var list = new List<AppItem>();
            using var doc = JsonDocument.Parse(json);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                var el = prop.Value;
                list.Add(new AppItem
                {
                    Id = prop.Name,
                    Name = GetString(el, "content"),
                    Category = GetString(el, "category"),
                    Description = GetString(el, "description"),
                    Choco = GetString(el, "choco"),
                    Winget = GetString(el, "winget"),
                    Link = GetString(el, "link"),
                    Foss = el.TryGetProperty("foss", out var f) && f.ValueKind == JsonValueKind.True,
                });
            }
            return list.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase).ToList();
        }
        private static string GetString(JsonElement el, string name)
            => el.TryGetProperty(name, out var v) ? v.GetString() ?? string.Empty : string.Empty;
        private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected) SwitchToPage("Settings");
            else if (args.SelectedItem is NavigationViewItem item && item.Tag is string tag) SwitchToPage(tag);
        }
        private void SwitchToPage(string pageTag)
        {
            BoosterPage.Visibility = Visibility.Collapsed;
            DebloaterPage.Visibility = Visibility.Collapsed;
            AppsInstallerPage.Visibility = Visibility.Collapsed;
            SettingsPage.Visibility = Visibility.Collapsed;
            switch (pageTag)
            {
                case "Booster": BoosterPage.Visibility = Visibility.Visible; break;
                case "Debloater": DebloaterPage.Visibility = Visibility.Visible; break;
                case "AppsInstaller": AppsInstallerPage.Visibility = Visibility.Visible; break;
                case "Settings": SettingsPage.Visibility = Visibility.Visible; break;
            }
        }
        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ThemeComboBox == null || Content is not FrameworkElement root) return;
            string? theme = (ThemeComboBox.SelectedItem as ComboBoxItem)?.Content as string;
            root.RequestedTheme = theme switch
            {
                "Light" => ElementTheme.Light,
                "Dark" => ElementTheme.Dark,
                _ => ElementTheme.Default,
            };
        }
        private void AppsSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (!_appsLoaded) return;
            string q = sender.Text?.Trim() ?? string.Empty;
            FilteredApps.Clear();
            IEnumerable<AppItem> filtered = string.IsNullOrEmpty(q)
                ? _allApps
                : _allApps.Where(a =>
                    a.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    a.Description.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                    a.Category.Contains(q, StringComparison.OrdinalIgnoreCase));
            foreach (var a in filtered) FilteredApps.Add(a);
        }
        private async void InstallSingleApp_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button b || b.Tag is not string id) return;
            var app = _allApps.FirstOrDefault(a => a.Id == id);
            if (app == null) return;
            if (JobQueue.Apps.Pending > 0) SetAppsStatus($"Queued: install {app.Name}…");
            await JobQueue.Apps.Enqueue($"Install {app.Name}", () =>
                RunAppsStatusAsync($"Installing {app.Name}...",
                    async () => await Task.Run(() => InstallApp(app))));
        }
        private async void InstallSelectedApps_Click(object sender, RoutedEventArgs e)
        {
            var selected = _allApps.Where(a => a.Selected).ToList();
            if (selected.Count == 0) { SetAppsStatus("No apps selected."); return; }
            if (JobQueue.Apps.Pending > 0) SetAppsStatus($"Queued: install {selected.Count} app(s)…");
            await JobQueue.Apps.Enqueue($"Install {selected.Count} app(s)", () =>
                RunAppsStatusAsync($"Installing {selected.Count} app(s)...", async () =>
                {
                    using var sem = new SemaphoreSlim(3);
                    var tasks = selected.Select(async app =>
                    {
                        await sem.WaitAsync();
                        try { await Task.Run(() => InstallApp(app)); }
                        finally { sem.Release(); }
                    });
                    await Task.WhenAll(tasks);
                }));
        }
        private async void InstallChocolatey_Click(object sender, RoutedEventArgs e)
        {
            const string command =
                "Set-ExecutionPolicy Bypass -Scope Process -Force; " +
                "[System.Net.ServicePointManager]::SecurityProtocol = [System.Net.ServicePointManager]::SecurityProtocol -bor 3072; " +
                "iex ((New-Object System.Net.WebClient).DownloadString('https://community.chocolatey.org/install.ps1'))";
            if (JobQueue.Apps.Pending > 0) SetAppsStatus("Queued: install Chocolatey…");
            await JobQueue.Apps.Enqueue("Install Chocolatey", () =>
                RunAppsStatusAsync("Installing Chocolatey...",
                    async () => await ScriptRunner.RunInlinePowerShellAsync(command)));
        }
        private static void InstallApp(AppItem app)
        {
            if (!string.IsNullOrWhiteSpace(app.Winget))
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "winget",
                    Arguments = $"install --id \"{app.Winget}\" -e --silent --accept-package-agreements --accept-source-agreements",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false,
                };
                try
                {
                    using var proc = Process.Start(psi);
                    if (proc != null && !proc.WaitForExit(15 * 60 * 1000))
                    {
                        Logger.Warn($"winget install '{app.Name}' exceeded 15 min - killing");
                        try { proc.Kill(true); } catch { }
                    }
                }
                catch (Exception ex) { Debug.WriteLine($"winget {app.Name}: {ex.Message}"); }
            }
        }
        private async void ExecuteBooster_Click(object sender, RoutedEventArgs e)
        {
            var toRun = _allBoosterCards.Where(c => c.Enabled).ToList();
            if (toRun.Count == 0) { SetBoosterStatus("Nothing toggled on."); return; }

            BoosterExecuteButton.IsEnabled = false;
            if (JobQueue.Instance.Pending > 0) SetBoosterStatus("Queued behind running work…");
            await JobQueue.Instance.Enqueue("Booster tweaks", () =>
                RunBoosterStatusAsync($"Running {toRun.Count} tweak(s)...", async () =>
                {
                    var tasks = toRun.Select(card => RunCardAsync(card));
                    await Task.WhenAll(tasks);
                }));
            BoosterExecuteButton.IsEnabled = true;
        }
        private async Task RunCardAsync(BoosterCard card)
        {
            DispatcherQueue.TryEnqueue(() => card.Running = true);
            try
            {
                if (card.WinUtilTweaks.Length > 0)
                    await ScriptRunner.RunWinUtilTweaksAsync(card.WinUtilTweaks);
                if (card.Win11DebloatArgs.Length > 0)
                    await ScriptRunner.RunWin11DebloatArgsAsync(card.Win11DebloatArgs);
                foreach (var act in card.EmbeddedActions)
                    await EmbeddedActions.RunAsync(act);
            }
            catch (Exception ex) { Debug.WriteLine($"Card '{card.Title}': {ex.Message}"); }
            finally { DispatcherQueue.TryEnqueue(() => card.Running = false); }
        }
        private void SelectRecommendedBooster_Click(object sender, RoutedEventArgs e)
        {
            foreach (var c in _allBoosterCards) c.Enabled = c.Recommended;
        }
        private void SelectAllBooster_Click(object sender, RoutedEventArgs e)
        {
            foreach (var c in _allBoosterCards) c.Enabled = true;
        }
        private void DeselectAllBooster_Click(object sender, RoutedEventArgs e)
        {
            foreach (var c in _allBoosterCards) c.Enabled = false;
        }
        private void BoosterSearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason != AutoSuggestionBoxTextChangeReason.UserInput) return;
            string q = sender.Text?.Trim() ?? string.Empty;
            IEnumerable<BoosterCard> matches = _allBoosterCards;
            if (q.Length > 0)
                matches = _allBoosterCards.Where(c =>
                    (c.Title?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Description?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Functions?.Contains(q, StringComparison.OrdinalIgnoreCase) ?? false));
            BoosterCards.Clear();
            foreach (var c in matches) BoosterCards.Add(c);
        }
        private void LoggingEnabled_Changed(object sender, RoutedEventArgs e)
        {
            bool on = LoggingEnabledCheck.IsChecked == true;
            Logger.Enabled = on;
            Logger.Info($"Logging toggled by user → {(on ? "ON" : "OFF")}");
        }
        private void OpenCurrentLog_Click(object sender, RoutedEventArgs e) => Logger.OpenCurrent();
        private void OpenLogFolder_Click(object sender, RoutedEventArgs e) => Logger.OpenFolder();
        private async void ExecuteDebloater_Click(object sender, RoutedEventArgs e)
        {
            var jobs = new List<DebloaterId>();
            if (DebloatCtt.IsChecked == true) jobs.Add(DebloaterId.CttTasks);
            if (DebloatRaphi.IsChecked == true) jobs.Add(DebloaterId.RaphiDebloat);
            if (DebloatApps.IsChecked == true) jobs.Add(DebloaterId.AppUninstaller);
            if (DebloatEdge.IsChecked == true) jobs.Add(DebloaterId.EdgeRemover);
            if (DebloatOb.IsChecked == true) jobs.Add(DebloaterId.OrangBoosterTasks);
            if (jobs.Count == 0) { SetDebloaterStatus("Nothing selected."); return; }
            await ExecuteDebloaterJobsAsync(jobs);
        }
        private async void NukeDebloater_Click(object sender, RoutedEventArgs e)
        {
            DebloatCtt.IsChecked = true;
            DebloatRaphi.IsChecked = true;
            DebloatApps.IsChecked = true;
            DebloatEdge.IsChecked = true;
            DebloatOb.IsChecked = true;
            var jobs = new List<DebloaterId>
            {
                DebloaterId.OrangBoosterTasks,
                DebloaterId.RaphiDebloat,
                DebloaterId.CttTasks,
                DebloaterId.AppUninstaller,
                DebloaterId.EdgeRemover,
            };
            Logger.Info("Nuclear Debloat triggered - running all five jobs sequentially");
            await ExecuteDebloaterJobsAsync(jobs);
        }
        private async Task ExecuteDebloaterJobsAsync(List<DebloaterId> jobs)
        {
            DebloaterExecuteButton.IsEnabled = false;
            DebloaterNukeButton.IsEnabled = false;
            Logger.OnLine += OnDebloaterLogLine;
            if (JobQueue.Instance.Pending > 0) SetDebloaterStatus("Queued behind running work…");
            try
            {
                await JobQueue.Instance.Enqueue("Debloater jobs", () =>
                    RunDebloaterStatusAsync($"Running {jobs.Count} job(s)...", async () =>
                    {
                        int i = 0;
                        foreach (var j in jobs)
                        {
                            i++;
                            SetDebloaterStatus($"[{i}/{jobs.Count}] {j}");
                            int code = await DebloaterTasks.RunAsync(j);
                            SetDebloaterStatus($"[{i}/{jobs.Count}] {j} → exit {code}");
                        }
                        await EmbeddedActions.RunAsync("clean_temp_caches");
                    }));
            }
            finally
            {
                Logger.OnLine -= OnDebloaterLogLine;
                DispatcherQueue.TryEnqueue(() => DebloaterLogLine.Text = "");
                DebloaterExecuteButton.IsEnabled = true;
                DebloaterNukeButton.IsEnabled = true;
            }
        }
        private void OnDebloaterLogLine(LogLevel level, string line)
        {
            int mark = line.IndexOf("] OUT: ", StringComparison.Ordinal);
            if (mark < 0) mark = line.IndexOf("] ERR: ", StringComparison.Ordinal);
            if (mark < 0) return;
            string msg = line[(mark + "] OUT: ".Length)..].Trim();
            if (msg.Length == 0) return;
            DispatcherQueue.TryEnqueue(() => DebloaterLogLine.Text = msg);
        }
        private void DebloaterRecommended_Click(object sender, RoutedEventArgs e)
        {
            DebloatCtt.IsChecked = true;
            DebloatRaphi.IsChecked = true;
            DebloatApps.IsChecked = true;
            DebloatEdge.IsChecked = false;
            DebloatOb.IsChecked = true;
        }
        private void DebloaterSelectAll_Click(object sender, RoutedEventArgs e)
        {
            DebloatCtt.IsChecked = true;
            DebloatRaphi.IsChecked = true;
            DebloatApps.IsChecked = true;
            DebloatEdge.IsChecked = true;
            DebloatOb.IsChecked = true;
        }
        private async Task RunBoosterStatusAsync(string msg, Func<Task> work)
        {
            DispatcherQueue.TryEnqueue(() => { BoosterProgress.IsActive = true; BoosterStatus.Text = msg; });
            try { await work(); SetBoosterStatus("Done."); }
            catch (Exception ex) { SetBoosterStatus($"Error: {ex.Message}"); }
            finally { DispatcherQueue.TryEnqueue(() => BoosterProgress.IsActive = false); }
        }
        private void SetBoosterStatus(string msg) => DispatcherQueue.TryEnqueue(() => BoosterStatus.Text = msg);
        private async Task RunDebloaterStatusAsync(string msg, Func<Task> work)
        {
            DispatcherQueue.TryEnqueue(() => { DebloaterProgress.IsActive = true; DebloaterStatus.Text = msg; });
            try { await work(); SetDebloaterStatus("Done."); }
            catch (Exception ex) { SetDebloaterStatus($"Error: {ex.Message}"); }
            finally { DispatcherQueue.TryEnqueue(() => DebloaterProgress.IsActive = false); }
        }
        private void SetDebloaterStatus(string msg) => DispatcherQueue.TryEnqueue(() => DebloaterStatus.Text = msg);
        private async Task RunAppsStatusAsync(string msg, Func<Task> work)
        {
            DispatcherQueue.TryEnqueue(() => { AppsProgressRing.IsActive = true; AppsStatusText.Text = msg; });
            try { await work(); SetAppsStatus("Done."); }
            catch (Exception ex) { SetAppsStatus($"Error: {ex.Message}"); }
            finally { DispatcherQueue.TryEnqueue(() => AppsProgressRing.IsActive = false); }
        }
        private void SetAppsStatus(string msg) => DispatcherQueue.TryEnqueue(() => AppsStatusText.Text = msg);
        private static Microsoft.UI.Xaml.Media.Imaging.BitmapImage? LoadAssetImage(string fileName)
        {
            string path = Path.Combine(AppContext.BaseDirectory, "Assets", fileName);
            if (!File.Exists(path)) { Logger.Warn($"Asset image missing: {path}"); return null; }
            return new Microsoft.UI.Xaml.Media.Imaging.BitmapImage(new Uri(path));
        }
        private void ApplyBackdrop()
        {
            try { SystemBackdrop = new Microsoft.UI.Xaml.Media.DesktopAcrylicBackdrop(); }
            catch (Exception ex) { Debug.WriteLine($"Backdrop: {ex.Message}"); }
}   }   }