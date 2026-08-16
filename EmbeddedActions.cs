using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
namespace OrangBooster
{
    public static class EmbeddedActions
    {
        static readonly RegistryKey HKLM = Registry.LocalMachine;
        static readonly RegistryKey HKCU = Registry.CurrentUser;
        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool SystemParametersInfo(uint uAction, uint uParam, string lpvParam, uint fuWinIni);
        const uint SPI_SETDESKWALLPAPER = 0x0014;
        const uint SPIF_UPDATEINIFILE = 0x01;
        const uint SPIF_SENDWININICHANGE = 0x02;
        static int Reg(params (RegistryKey root, string path, string name, object value)[] entries)
        {
            try
            {
                foreach (var (root, path, name, value) in entries)
                {
                    using var k = root.CreateSubKey(path, true);
                    k?.SetValue(name, value, value is string ? RegistryValueKind.String : RegistryValueKind.DWord);
                }
                return 0;
            }
            catch (Exception ex) { Logger.Error("Reg", ex); return -1; }
        }
        public static async Task<int> RunAsync(string id, CancellationToken ct = default)
        {
            Logger.Info($"EmbeddedAction '{id}' START");
            int code;
            try
            {
                code = id switch
                {
                    "cortana_remove"            => await CortanaRemoveAsync(ct),
                    "tcp_nagle"                 => await TcpNagleAsync(ct),
                    "net_stack_reset"           => await NetStackResetAsync(ct),
                    "net_sparkle_tweaks"        => await NetSparkleTweaksAsync(ct),
                    "dism_strip_features"       => await DismStripFeaturesAsync(ct),
                    "advertising_id_off"        => await AdvertisingIdOffAsync(ct),
                    "appx_bloat_remove"         => await AppxBloatRemoveAsync(ct),
                    "perf_responsiveness"       => await PerfResponsivenessAsync(ct),
                    "all_a11y_off"              => await AccessibilityKeysOffAsync(ct),
                    "explorer_home_gallery_off" => await ExplorerHomeGalleryOffAsync(ct),
                    "reserved_storage_off"      => await ReservedStorageOffAsync(ct),
                    "raven_uninstall_oo"        => await OneDriveOutlookRemoveAsync(ct),
                    "oo_remove_robust"          => await OneDriveOutlookRemoveAsync(ct),
                    "raven_update_policy"       => await RavenUpdatePolicyAsync(ct),
                    "raven_update_policy_pro"   => await RavenUpdatePolicyProAsync(ct),
                    "edge_remover_bat"          => await EdgeRemoverBatAsync(ct),
                    "ob_registry_pack"          => await ObRegistryPackAsync(ct),
                    "ob_full_pack"              => await ObFullPackAsync(ct),
                    "nuke_ai_copilot"           => await NukeAiCopilotAsync(ct),
                    "force_dark_no_spotlight"   => await ForceDarkNoSpotlightAsync(ct),
                    "clock_24h"                 => await Clock24HourAsync(ct),
                    "clock_seconds"             => await ClockSecondsAsync(ct),
                    "enable_hags"               => await EnableHagsAsync(ct),
                    "enable_game_mode"          => await EnableGameModeAsync(ct),
                    "dx_windowed_opt"           => await DxWindowedOptAsync(ct),
                    "disable_core_isolation"    => await DisableCoreIsolationAsync(ct),
                    "disable_dynamic_ticking"   => await DisableDynamicTickingAsync(ct),
                    "disable_fast_startup"      => await DisableFastStartupAsync(ct),
                    "disable_wifi_sense"        => await DisableWifiSenseAsync(ct),
                    "set_time_utc"              => await SetTimeUtcAsync(ct),
                    "priority_separation_fg"    => await PrioritySeparationForegroundAsync(ct),
                    "disable_rdp_warnings"      => await DisableRdpWarningsAsync(ct),
                    "disable_widgets_taskview"  => await DisableWidgetsTaskViewAsync(ct),
                    "verbose_bsod"              => await VerboseBsodAsync(ct),
                    "remove_onedrive"           => await OneDriveOutlookRemoveAsync(ct),
                    "privacy_pack"              => await PrivacyPackAsync(ct),
                    "explorer_view_tweaks"      => await ExplorerViewTweaksAsync(ct),
                    "clipboard_history_on"      => await ClipboardHistoryOnAsync(ct),
                    "long_paths_on"             => await LongPathsOnAsync(ct),
                    "set_dns_cloudflare"        => await SetDnsCloudflareAsync(ct),
                    "net_bindings_off"          => await NetBindingsOffAsync(ct),
                    "power_ultimate"            => await PowerUltimateAsync(ct),
                    "display_max_refresh"       => await DisplayMaxRefreshAsync(ct),
                    "disable_disk_encryption"   => await DisableDiskEncryptionAsync(ct),
                    "disable_ucpd"              => await DisableUcpdAsync(ct),
                    "uninstall_terminal"        => await UninstallTerminalAsync(ct),
                    "remove_rdp_shortcuts"      => await RemoveRdpShortcutsAsync(ct),
                    "bing_websearch_off"        => await BingWebSearchOffAsync(ct),
                    "remove_capabilities_pack"  => await RemoveCapabilitiesPackAsync(ct),
                    "disable_features_pack"     => await DisableFeaturesPackAsync(ct),
                    "clean_temp_caches"         => await CleanTempCachesAsync(ct),
                    "mouse_accel_off"           => await MouseAccelOffAsync(ct),
                    "restart_explorer"          => await RestartExplorerAsync(ct),
                    "gamebar_off"               => await GameBarOffAsync(ct),
                    "xbox_services_off"         => await XboxServicesOffAsync(ct),
                    "minimize_services"         => await MinimizeServicesAsync(ct),
                    "unpin_store"               => await UnpinStoreAsync(ct),
                    "clear_start_pins"          => await ClearStartPinsAsync(ct),
                    "clear_taskbar_pins"        => await ClearTaskbarPinsAsync(ct),
                    "remove_store"              => await RemoveStoreAsync(ct),
                    "copilot_spyware_off"       => await CopilotSpywareOffAsync(ct),
                    "lockscreen_start_clean"    => await LockscreenStartCleanAsync(ct),
                    "normal_shutdown_dialog"    => await NormalShutdownDialogAsync(ct),
                    "account_nags_off"          => await AccountNagsOffAsync(ct),
                    "settings_home_off"         => await SettingsHomeOffAsync(ct),
                    "xbox_remove_full"          => await XboxRemoveFullAsync(ct),
                    "edge_remove_full"          => await EdgeRemoveFullAsync(ct),
                    "ai_full_policies"          => await AiFullPoliciesAsync(ct),
                    "paint_ai_off"              => await PaintAiOffAsync(ct),
                    "device_companion_off"      => await DeviceCompanionOffAsync(ct),
                    "oem_freeware_remove"       => await OemFreewareRemoveAsync(ct),
                    "ps7_telemetry_off"         => await Ps7TelemetryOffAsync(ct),
                    "gaming_perf_pack"          => await GamingPerfPackAsync(ct),
                    _ => -1,
                };
            }
            catch (Exception ex)
            {
                Logger.Error($"EmbeddedAction '{id}' threw", ex);
                return -1;
            }
            Logger.Info($"EmbeddedAction '{id}' DONE (code={code})");
            return code;
        }
        static Task<int> CortanaRemoveAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            Write-Output 'Removing Cortana appx packages'
            Get-AppxPackage -allusers Microsoft.549981C3F5F10 | Remove-AppxPackage -AllUsers
            Get-AppxPackage -allusers *Cortana* | Remove-AppxPackage -AllUsers
            $rk='HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Search'
            if(-not (Test-Path $rk)) { New-Item -Path $rk -Force | Out-Null }
            Set-ItemProperty -Path $rk -Name 'AllowCortana' -Type DWord -Value 0 -Force
            Set-ItemProperty -Path $rk -Name 'DisableWebSearch' -Type DWord -Value 1 -Force
            Set-ItemProperty -Path $rk -Name 'ConnectedSearchUseWeb' -Type DWord -Value 0 -Force
            Write-Output 'Cortana policy keys set'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "cortana", ct);
        }
        static Task<int> TcpNagleAsync(CancellationToken ct) => Task.Run(() =>
        {
            try
            {
                string[] roots =
                {
                    @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters\Interfaces",
                    @"SOFTWARE\Microsoft\MSMQ\Parameters",
                };
                int touched = 0;
                foreach (var root in roots)
                {
                    using var key = HKLM.OpenSubKey(root);
                    if (key == null) continue;
                    foreach (var sub in key.GetSubKeyNames())
                    {
                        using var sk = HKLM.OpenSubKey($"{root}\\{sub}", true);
                        if (sk == null) continue;
                        sk.SetValue("TCPNoDelay", 1, RegistryValueKind.DWord);
                        sk.SetValue("TcpAckFrequency", 1, RegistryValueKind.DWord);
                        sk.SetValue("TcpDelAckTicks", 0, RegistryValueKind.DWord);
                        touched++;
                    }
                }
                using (var p = HKLM.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters", true))
                {
                    p?.SetValue("TcpNoDelay", 1, RegistryValueKind.DWord);
                    p?.SetValue("TcpAckFrequency", 1, RegistryValueKind.DWord);
                    p?.SetValue("TcpDelAckTicks", 0, RegistryValueKind.DWord);
                }
                using (var s = HKLM.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\LanmanServer\Parameters", true))
                {
                    s?.SetValue("IRPStackSize", 30, RegistryValueKind.DWord);
                }
                Logger.Info($"TCP/Nagle: tuned {touched} interface entries");
                return 0;
            }
            catch (Exception ex) { Logger.Error("TcpNagle", ex); return -1; }
        }, ct);
        static async Task<int> NetStackResetAsync(CancellationToken ct)
        {
            string[] cmds =
            {
                "netsh winsock reset",
                "netsh interface tcp set global autotuninglevel=disabled",
                "netsh interface tcp set global dca=enabled",
                "netsh interface tcp set global ecncapability=enabled",
                "netsh interface tcp set global timestamps=enabled",
                "netsh interface teredo set state disabled",
                "ipconfig /flushdns",
                "ipconfig /registerdns",
            };
            int worst = 0;
            foreach (var c in cmds)
            {
                int code = await ScriptRunner.RunHiddenAsync("cmd.exe", $"/c {c}", $"net:{c.Split(' ')[0]}", ct);
                if (code != 0) worst = code;
            }
            return worst;
        }
        static async Task<int> NetSparkleTweaksAsync(CancellationToken ct)
        {
            string[] cmds =
            {
                "netsh int tcp set heuristics disabled",
                "netsh int tcp set supplemental template=internet congestionprovider=ctcp",
                "netsh int tcp set global rss=enabled",
                "netsh int tcp set global ecncapability=enabled",
                "netsh int tcp set global timestamps=disabled",
                "netsh int tcp set global fastopen=enabled",
                "netsh int tcp set global fastopenfallback=enabled",
                "netsh int tcp set supplemental template=custom icw=10",
            };
            int worst = 0;
            foreach (var c in cmds)
            {
                int code = await ScriptRunner.RunHiddenAsync("cmd.exe", $"/c {c}", "net-sparkle", ct);
                if (code != 0) worst = code;
            }
            return worst;
        }
        static async Task<int> DismStripFeaturesAsync(CancellationToken ct)
        {
            string[] features =
            {
                "WindowsMediaPlayer", "Printing-PrintToPDFServices-Features",
                "Printing-XPSServices-Features", "WorkFolders-Client", "Recall",
            };
            int worst = 0;
            foreach (var f in features)
            {
                int code = await ScriptRunner.RunHiddenAsync("dism.exe",
                    $"/online /disable-feature /featurename:{f} /norestart", $"dism:{f}", ct);
                if (code != 0 && code != 50) worst = code;
            }
            return worst;
        }
        static Task<int> AdvertisingIdOffAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\AdvertisingInfo", "Enabled", 0)
        ), ct);
        public static readonly string[] BloatwareAppx =
        {
            "Microsoft.3DBuilder", "Microsoft.Microsoft3DViewer", "Microsoft.AppConnector",
            "Microsoft.BingFinance", "Microsoft.BingNews", "Microsoft.BingSports",
            "Microsoft.BingTranslator", "Microsoft.BingWeather", "Microsoft.BingFoodAndDrink",
            "Microsoft.BingHealthAndFitness", "Microsoft.BingTravel", "Microsoft.BingSearch",
            "Clipchamp.Clipchamp", "Microsoft.Todos", "MSTeams",
            "MicrosoftCorporationII.QuickAssist", "Microsoft.MinecraftUWP", "Microsoft.GamingServices",
            "Microsoft.GetHelp", "Microsoft.Getstarted", "Microsoft.Messaging",
            "Microsoft.MicrosoftSolitaireCollection", "Microsoft.MicrosoftStickyNotes",
            "Microsoft.MixedReality.Portal", "Microsoft.NetworkSpeedTest", "Microsoft.News",
            "Microsoft.Office.Lens", "Microsoft.Office.Sway", "Microsoft.Office.OneNote",
            "Microsoft.OneConnect", "Microsoft.People", "Microsoft.Print3D",
            "Microsoft.SkypeApp", "Microsoft.Wallet", "Microsoft.Whiteboard",
            "Microsoft.WindowsAlarms", "Microsoft.WindowsCamera", "microsoft.windowscommunicationsapps",
            "Microsoft.WindowsFeedbackHub", "Microsoft.WindowsMaps", "Microsoft.WindowsPhone",
            "Microsoft.WindowsSoundRecorder", "Microsoft.XboxApp", "Microsoft.ConnectivityStore",
            "Microsoft.CommsPhone", "Microsoft.Xbox.TCUI", "Microsoft.XboxGameOverlay",
            "Microsoft.XboxGameCallableUI", "Microsoft.XboxSpeechToTextOverlay",
            "Microsoft.XboxIdentityProvider", "Microsoft.ZuneVideo", "Microsoft.YourPhone",
            "Microsoft.MicrosoftOfficeHub", "Microsoft.MicrosoftEdgeDevToolsClient",
            "Microsoft.Windows.ContentDeliveryManager", "Microsoft.Windows.SecureAssessmentBrowser",
            "Microsoft.Windows.NarratorQuickStart", "Microsoft.PowerAutomateDesktop",
            "MicrosoftWindows.CrossDevice", "Microsoft.Windows.DevHome",
            "Microsoft.ApplicationCompatibilityEnhancements", "Microsoft.Edge.GameAssist",
            "Microsoft.ScreenSketch", "MicrosoftWindows.Client.WebExperience",
            "MicrosoftWindows.Client.Photon", "Microsoft.Windows.ParentalControls",
            "Microsoft.VP9VideoExtensions", "Microsoft.XboxGamingOverlay",
            "Microsoft.EdgeDevtoolsPlugin", "Microsoft.Advertising.Xaml",
            "Microsoft.Windows.Cortana", "Microsoft.OutlookForWindows",
            "*EclipseManager*", "*ActiproSoftwareLLC*",
            "*AdobeSystemsIncorporated.AdobePhotoshopExpress*", "*Duolingo-LearnLanguagesforFree*",
            "*PandoraMediaInc*", "*CandyCrush*", "*BubbleWitch3Saga*", "*Wunderlist*",
            "*Flipboard*", "*Twitter*", "*Facebook*", "*Royal Revolt*", "*Sway*",
            "*Speed Test*", "*Dolby*", "*Viber*", "*ACGMediaPlayer*", "*Netflix*",
            "*OneCalendar*", "*LinkedInforWindows*", "*HiddenCityMysteryofShadows*",
            "*Hulu*", "*HiddenCity*", "*AdobePhotoshopExpress*", "*HotspotShieldFreeVPN*",
        };
        static Task<int> AppxBloatRemoveAsync(CancellationToken ct)
        {
            var list = string.Join(",", System.Array.ConvertAll(BloatwareAppx, a => "'" + a.Replace("'", "''") + "'"));
            string script = $@"
            $ErrorActionPreference='SilentlyContinue'
            $apps = @({list})
            $prov = Get-AppxProvisionedPackage -Online
            Write-Output (""Sweeping "" + $apps.Count + "" bloatware packages"")
            foreach ($a in $apps) {{
            Write-Output ""Removing $a""
            Get-AppxPackage -AllUsers $a | Remove-AppxPackage -AllUsers
            $prov | Where-Object {{ $_.PackageName -like ""$a*"" }} | Remove-AppxProvisionedPackage -Online -AllUsers | Out-Null
            }}
            Write-Output 'Sweep complete'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "appx-bloat", ct);
        }
        static Task<int> PerfResponsivenessAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Control Panel\Desktop", "MenuShowDelay", "0"),
            (HKCU, @"Control Panel\Desktop", "WaitToKillAppTimeout", "2000"),
            (HKCU, @"Control Panel\Desktop", "HungAppTimeout", "1000"),
            (HKCU, @"Control Panel\Desktop", "AutoEndTasks", "1"),
            (HKLM, @"SYSTEM\CurrentControlSet\Control", "WaitToKillServiceTimeout", "2000"),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "NoLowDiskSpaceChecks", 1)
        ), ct);
        static Task<int> AccessibilityKeysOffAsync(CancellationToken ct) => Task.Run(() =>
        {
            try
            {
                (string sub, (string name, int v)[] vals)[] groups =
                {
                    (@"Control Panel\Accessibility\StickyKeys", new[] { ("Flags", 506), ("HotKeyActive", 0), ("HotKeySound", 0), ("ConfirmOnHotKey", 0) }),
                    (@"Control Panel\Accessibility\Keyboard Response", new[] { ("Flags", 122), ("HotKeyActive", 0), ("HotKeySound", 0), ("ConfirmOnHotKey", 0) }),
                    (@"Control Panel\Accessibility\ToggleKeys", new[] { ("Flags", 58), ("HotKeyActive", 0), ("HotKeySound", 0), ("ConfirmOnHotKey", 0) }),
                    (@"Control Panel\Accessibility\FilterKeys", new[] { ("Flags", 34), ("HotKeyActive", 0), ("HotKeySound", 0), ("ConfirmOnHotKey", 0) }),
                };
                foreach (var (sub, vals) in groups)
                {
                    using var k = HKCU.CreateSubKey(sub, true);
                    foreach (var (n, v) in vals) k?.SetValue(n, v.ToString(), RegistryValueKind.String);
                }
                return 0;
            }
            catch (Exception ex) { Logger.Error("A11y", ex); return -1; }
        }, ct);
        static Task<int> ExplorerHomeGalleryOffAsync(CancellationToken ct) => Task.Run(() =>
        {
            try
            {
                Reg(
                    (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer", "HubMode", 1),
                    (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "LaunchTo", 1)
                );
                string[] toDelete =
                {
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{e88865ea-0e1c-4e20-9aa6-edcd0212c87c}",
                    @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{f874310e-b6b7-47dc-bc84-b9e6b38f5903}",
                };
                foreach (var p in toDelete)
                {
                    try { HKLM.DeleteSubKeyTree(p, false); } catch { }
                }
                return 0;
            }
            catch (Exception ex) { Logger.Error("Explorer", ex); return -1; }
        }, ct);
        static Task<int> ReservedStorageOffAsync(CancellationToken ct)
            => ScriptRunner.RunInlinePowerShellAsync(
                "Set-WindowsReservedStorageState -State Disabled -ErrorAction SilentlyContinue", "reserved-off", ct);
        static Task<int> ObRegistryPackAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ExtendedUIHoverTime", 1),
            (HKLM, @"SYSTEM\CurrentControlSet\Control\CrashControl", "DisplayParameters", 4),
            (HKLM, @"SYSTEM\CurrentControlSet\Control\CrashControl", "DisablePromo", 1),
            (HKLM, @"SYSTEM\CurrentControlSet\Control\FileSystem", "LongPathsEnabled", 1),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\DeliveryOptimization", "DODownloadMode", 0),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\DeliveryOptimization\Config", "DODownloadMode", 0)
        ), ct);
        static async Task<int> ObFullPackAsync(CancellationToken ct)
        {
            var steps = new[] {
                "ob_registry_pack", "force_dark_no_spotlight", "nuke_ai_copilot",
                "perf_responsiveness", "advertising_id_off", "all_a11y_off",
                "mouse_accel_off", "gamebar_off", "privacy_pack", "explorer_view_tweaks",
                "account_nags_off", "settings_home_off",
                "explorer_home_gallery_off", "clipboard_history_on", "long_paths_on",
                "set_dns_cloudflare", "net_bindings_off", "power_ultimate", "display_max_refresh",
                "disable_disk_encryption", "disable_ucpd", "uninstall_terminal", "remove_rdp_shortcuts",
                "bing_websearch_off", "remove_onedrive", "xbox_services_off", "minimize_services",
                "ai_full_policies", "paint_ai_off", "device_companion_off", "gaming_perf_pack",
                "unpin_store", "clear_start_pins", "clear_taskbar_pins", "remove_capabilities_pack", "disable_features_pack" };
            var failed = new System.Collections.Generic.List<string>();
            foreach (var step in steps)
            {
                ct.ThrowIfCancellationRequested();
                Logger.Info($"ob_full_pack step → {step}");
                int code;
                try { code = await RunAsync(step, ct); }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex) { Logger.Error($"ob_full_pack step '{step}' threw", ex); code = -1; }
                if (code != 0) failed.Add($"{step}({code})");
            }
            if (failed.Count > 0)
                Logger.Warn($"ob_full_pack completed with {failed.Count} best-effort step(s) not fully applied: {string.Join(", ", failed)}");
            else
                Logger.Info("ob_full_pack completed, all steps applied");
            return 0;
        }
        static readonly string DarkThemeFile = @"C:\Windows\Resources\Themes\dark.theme";
        static void SetDefaultDarkWallpaper()
        {
            try
            {
                string[] candidates =
                {
                    @"C:\Windows\Web\Wallpaper\Windows\img19.jpg",
                    @"C:\Windows\Web\Wallpaper\Windows\img0.jpg",
                };
                string? wp = null;
                foreach (var c in candidates) { if (File.Exists(c)) { wp = c; break; } }
                if (wp == null) { Logger.Warn("No default wallpaper found to apply"); return; }
                Reg(
                    (HKCU, @"Control Panel\Desktop", "WallpaperStyle", "10"),
                    (HKCU, @"Control Panel\Desktop", "TileWallpaper", "0"),
                    (HKCU, @"Control Panel\Desktop", "WallPaper", wp)
                );
                bool ok = SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, wp, SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE);
                Logger.Info($"Wallpaper set to {Path.GetFileName(wp)} (ok={ok})");
            }
            catch (Exception ex) { Logger.Warn($"Wallpaper set failed: {ex.Message}"); }
        }
        static void ApplyThemeFile(string themePath)
        {
            if (!File.Exists(themePath)) { Logger.Warn($"Theme file missing: {themePath}"); return; }
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = themePath,
                    UseShellExecute = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                });
                for (int i = 0; i < 30; i++)
                {
                    Thread.Sleep(100);
                    var found = Process.GetProcessesByName("SystemSettings");
                    foreach (var s in found) s.Dispose();
                    if (found.Length == 0) continue;
                    Thread.Sleep(200);
                    foreach (var s in Process.GetProcessesByName("SystemSettings"))
                    {
                        try { s.Kill(); } catch { }
                        s.Dispose();
                    }
                    break;
                }
                Logger.Info($"Applied theme {Path.GetFileName(themePath)}");
            }
            catch (Exception ex) { Logger.Warn($"Theme apply failed: {ex.Message}"); }
        }
        static Task<int> ForceDarkNoSpotlightAsync(CancellationToken ct) => Task.Run(() =>
        {
            int r = Reg(
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme", 0),
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme", 0),
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "EnableTransparency", 1),
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenEnabled", 0),
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "RotatingLockScreenOverlayEnabled", 0),
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338387Enabled", 0),
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338388Enabled", 0),
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0),
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-310093Enabled", 0),
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SystemPaneSuggestionsEnabled", 0),
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0),
                (HKCU, @"Software\Policies\Microsoft\Windows\CloudContent", "DisableSpotlightCollectionOnDesktop", 1),
                (HKCU, @"Software\Policies\Microsoft\Windows\CloudContent", "DisableWindowsSpotlightFeatures", 1),
                (HKCU, @"Software\Policies\Microsoft\Windows\CloudContent", "DisableWindowsSpotlightOnActionCenter", 1),
                (HKCU, @"Software\Policies\Microsoft\Windows\CloudContent", "DisableWindowsSpotlightOnSettings", 1),
                (HKCU, @"Software\Policies\Microsoft\Windows\CloudContent", "DisableWindowsSpotlightWindowsWelcomeExperience", 1),
                (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableWindowsSpotlightFeatures", 1),
                (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\CloudContent", "DisableSpotlightCollectionOnDesktop", 1)
            );
            ApplyThemeFile(DarkThemeFile);
            SetDefaultDarkWallpaper();
            Logger.Info("Dark mode forced + wallpaper + Spotlight wiped");
            return r;
        }, ct);
        static Task<int> NukeAiCopilotAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference = 'SilentlyContinue'
            Write-Output 'Cleaning main slop'
            $pkgs = @(
            '*Microsoft.Copilot*',
            '*Microsoft.MicrosoftOfficeHub*',
            '*MicrosoftWindows.Client.CoreAI*',
            '*Microsoft.Windows.Ai.Copilot.Provider*'
            )
            foreach ($p in $pkgs) {
                Get-AppxPackage -AllUsers $p | ForEach-Object {
                    Write-Output ""Removing $($_.PackageFullName)""
                    Remove-AppxPackage -Package $_.PackageFullName -AllUsers
                }
                Get-AppxProvisionedPackage -Online |
                    Where-Object { $_.PackageName -like $p } |
                    ForEach-Object {
                        Write-Output ""Removing provisioned $($_.PackageName)""
                        Remove-AppxProvisionedPackage -Online -AllUsers -PackageName $_.PackageName | Out-Null
                    }
            }
            foreach ($svc in 'WSAIFabricSvc','CopilotService') {
                if (Get-Service -Name $svc -ErrorAction SilentlyContinue) {
                    Write-Output ""Disabling service $svc""
                    & sc.exe stop $svc | Out-Null
                    & sc.exe config $svc start= disabled | Out-Null
                }
            }
            Disable-WindowsOptionalFeature -Online -FeatureName 'Recall' -NoRestart -ErrorAction SilentlyContinue | Out-Null
            $pols = @{
                'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsAI' = @{
                    'DisableAIDataAnalysis' = 1
                    'DisableImageCreator' = 1
                    'AllowRecallEnablement' = 0
                    'TurnOffWindowsCopilot' = 1
                    'DisableClickToDo' = 1
                }
                'HKLM:\SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot' = @{
                    'TurnOffWindowsCopilot' = 1
                }
                'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint' = @{
                    'DisableImageCreator' = 1
                }
                'HKLM:\SOFTWARE\Policies\WindowsNotepad' = @{
                    'DisableAIFeatures' = 1
                }
                'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' = @{
                    'ShowCopilotButton' = 0
                }
                'HKCU:\Software\Microsoft\Windows\Shell\Copilot' = @{
                    'IsCopilotAvailable' = 0
                }
                'HKCU:\Software\Microsoft\Windows\Shell\Copilot\BingChat' = @{
                    'IsUserEligible' = 0
                }
                'HKCU:\Software\Policies\Microsoft\Windows\WindowsCopilot' = @{
                    'TurnOffWindowsCopilot' = 1
                }
                'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer' = @{
                    'SettingsPageVisibility' = 'hide:aicomponents'
                }
            }
            foreach ($k in $pols.Keys) {
                if (-not (Test-Path $k)) { New-Item -Path $k -Force | Out-Null }
                foreach ($n in $pols[$k].Keys) {
                    $v = $pols[$k][$n]
                    if ($v -is [int]) {
                        Set-ItemProperty -Path $k -Name $n -Value $v -Type DWord -Force
                    } else {
                        Set-ItemProperty -Path $k -Name $n -Value $v -Type String -Force
                    }
                    Write-Output ""Set $k\$n = $v""
                }
            }
            if (Get-Command winget -ErrorAction SilentlyContinue) {
                & winget uninstall --id Microsoft.Copilot_8wekyb3d8bbwe --silent --accept-source-agreements --disable-interactivity | Out-Null
            }
            Write-Output '=== AI / Copilot / Recall nuked ==='
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "nuke-ai", ct);
        }

        const string RavenUpdatePolicyUrl = "https://raw.githubusercontent.com/ravendevteam/talon/main/debloat_raven_scripts/update_policy_changer.ps1";
        const string RavenUpdatePolicyProUrl = "https://raw.githubusercontent.com/ravendevteam/talon/main/debloat_raven_scripts/update_policy_changer_pro.ps1";
        static Task<int> RavenUpdatePolicyAsync(CancellationToken ct)
            => ScriptRunner.RunRemoteScriptAsync(RavenUpdatePolicyUrl, "raven_update_policy.ps1", "", "raven:upd-home", ct);
        static Task<int> RavenUpdatePolicyProAsync(CancellationToken ct)
            => ScriptRunner.RunRemoteScriptAsync(RavenUpdatePolicyProUrl, "raven_update_policy_pro.ps1", "", "raven:upd-pro", ct);
        static Task<int> EdgeRemoverBatAsync(CancellationToken ct)
        {
            if (!File.Exists(ScriptRunner.EdgeBatPath))
            { Logger.Error($"edge.bat missing at {ScriptRunner.EdgeBatPath}"); return Task.FromResult(-1); }
            return ScriptRunner.RunBatchHiddenAsync(ScriptRunner.EdgeBatPath, "edge-bat", ct);
        }
        static Task<int> Clock24HourAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Control Panel\International", "sShortTime", "HH:mm"),
            (HKCU, @"Control Panel\International", "sTimeFormat", "HH:mm:ss")
        ), ct);
        static Task<int> ClockSecondsAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSecondsInSystemClock", 1)
        ), ct);
        static Task<int> EnableHagsAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SYSTEM\CurrentControlSet\Control\GraphicsDrivers", "HwSchMode", 2)
        ), ct);
        static Task<int> EnableGameModeAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Software\Microsoft\GameBar", "AllowAutoGameMode", 1),
            (HKCU, @"Software\Microsoft\GameBar", "AutoGameModeEnabled", 1)
        ), ct);
        static Task<int> DxWindowedOptAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Software\Microsoft\DirectX\UserGpuPreferences", "DirectXUserGlobalSettings", "SwapEffectUpgradeEnable=1;")
        ), ct);
        static Task<int> DisableCoreIsolationAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SYSTEM\CurrentControlSet\Control\DeviceGuard\Scenarios\HypervisorEnforcedCodeIntegrity", "Enabled", 0)
        ), ct);
        static Task<int> DisableDynamicTickingAsync(CancellationToken ct)
            => ScriptRunner.RunHiddenAsync("bcdedit.exe", "/set disabledynamictick yes", "bcdedit:dyntick", ct);
        static Task<int> DisableFastStartupAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SYSTEM\CurrentControlSet\Control\Session Manager\Power", "HiberbootEnabled", 0)
        ), ct);
        static Task<int> DisableWifiSenseAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"Software\Microsoft\PolicyManager\default\WiFi\AllowWiFiHotSpotReporting", "Value", 0),
            (HKLM, @"Software\Microsoft\PolicyManager\default\WiFi\AllowAutoConnectToWiFiSenseHotspots", "Value", 0)
        ), ct);
        static Task<int> SetTimeUtcAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SYSTEM\CurrentControlSet\Control\TimeZoneInformation", "RealTimeIsUniversal", 1)
        ), ct);
        static Task<int> PrioritySeparationForegroundAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SYSTEM\CurrentControlSet\Control\PriorityControl", "Win32PrioritySeparation", 36)
        ), ct);
        static Task<int> DisableRdpWarningsAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"Software\Policies\Microsoft\Windows NT\Terminal Services\Client", "RedirectionWarningDialogVersion", 1)
        ), ct);
        static Task<int> DisableWidgetsTaskViewAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            Set-ItemProperty -Path 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced' -Name 'ShowTaskViewButton' -Type DWord -Value 0
            Get-AppxPackage *WebExperience* | Remove-AppxPackage
            Get-Process explorer -ErrorAction SilentlyContinue | Stop-Process -Force
            Start-Process explorer
            Write-Output 'Widgets removed, Task View hidden'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "widgets-taskview", ct);
        }
        static Task<int> VerboseBsodAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SYSTEM\CurrentControlSet\Control\CrashControl", "DisplayParameters", 1),
            (HKLM, @"SYSTEM\CurrentControlSet\Control\CrashControl", "DisableEmoticon", 1)
        ), ct);
        static Task<int> OneDriveOutlookRemoveAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            Write-Output '=== OneDrive + New Outlook removal ==='
            foreach ($n in 'OneDrive','OneDriveSetup','FileCoAuth','OUTLOOK','olk','olk_native') {
                Get-Process -Name $n | Stop-Process -Force
            }
            Start-Sleep -Seconds 1
            $pf64 = $env:ProgramW6432; if (-not $pf64) { $pf64 = $env:ProgramFiles }
            $candidates = [System.Collections.Generic.List[string]]::new()
            @(
                ""$env:SystemRoot\System32\OneDriveSetup.exe"",
                ""$env:SystemRoot\SysWOW64\OneDriveSetup.exe"",
                ""$env:SystemRoot\Sysnative\OneDriveSetup.exe"",
                ""$pf64\Microsoft OneDrive\OneDrive.exe"",
                ""${env:ProgramFiles(x86)}\Microsoft OneDrive\OneDrive.exe"",
                ""$env:LOCALAPPDATA\Microsoft\OneDrive\OneDrive.exe"",
                ""$env:LOCALAPPDATA\Microsoft\OneDrive\Update\OneDriveSetup.exe""
            ) | ForEach-Object { $candidates.Add($_) }
            foreach ($base in @(""$pf64\Microsoft OneDrive"", ""${env:ProgramFiles(x86)}\Microsoft OneDrive"")) {
                if (Test-Path $base) {
                    Get-ChildItem -Path $base -Recurse -Filter 'OneDriveSetup.exe' -ErrorAction SilentlyContinue |
                        ForEach-Object { $candidates.Add($_.FullName) }
                }
            }
            $ran = $false
            foreach ($p in ($candidates | Select-Object -Unique)) {
                if (Test-Path $p) {
                    Write-Output ""Running OneDrive uninstaller: $p""
                    Start-Process $p -ArgumentList '/uninstall' -Wait -WindowStyle Hidden
                    $ran = $true
                }
            }
            if (-not $ran) {
                Write-Output 'No OneDrive uninstaller found, trying winget fallback'
                & winget uninstall --id Microsoft.OneDrive --silent --accept-source-agreements --disable-interactivity | Out-Null
            }

            foreach ($d in @(
                ""$env:USERPROFILE\OneDrive"",
                ""$env:LOCALAPPDATA\Microsoft\OneDrive"",
                ""$env:ProgramData\Microsoft OneDrive"",
                ""$env:SystemDrive\OneDriveTemp""
            )) {
                if (Test-Path $d) { Write-Output ""Removing $d""; Remove-Item $d -Recurse -Force }
            }

            foreach ($k in @(
                'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{018D5C66-4533-4307-9B53-224DE2ED1FE6}',
                'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\Desktop\NameSpace\{018D5C66-4533-4307-9B53-224DE2ED1FE6}'
            )) {
                if (Test-Path $k) { Write-Output ""Removing reg $k""; Remove-Item $k -Recurse -Force }
            }
            Set-Service -Name OneSyncSvc -StartupType Disabled
            Set-Service -Name OneDrive -StartupType Disabled

            foreach ($pkg in 'Microsoft.OutlookForWindows','Microsoft.Office.Outlook') {
                $found = Get-AppxPackage -AllUsers $pkg
                if ($found) {
                    Write-Output ""Removing appx $pkg""
                    $found | Remove-AppxPackage -AllUsers
                }
                Get-AppxProvisionedPackage -Online |
                    Where-Object { $_.PackageName -like ""$pkg*"" } |
                    ForEach-Object {
                        Write-Output ""Removing provisioned $($_.PackageName)""
                        Remove-AppxProvisionedPackage -Online -AllUsers -PackageName $_.PackageName | Out-Null
                    }
            }
            $wapps = ""$env:ProgramFiles\WindowsApps""
            if (Test-Path $wapps) {
                Get-ChildItem -Path $wapps -Directory |
                    Where-Object { $_.Name -like 'Microsoft.OutlookForWindows*' } |
                    ForEach-Object {
                        $p = $_.FullName
                        Write-Output ""Stripping $p""
                        takeown /f $p /r /d Y 2>$null | Out-Null
                        icacls $p /grant '*S-1-5-32-544:F' /t /c 2>$null | Out-Null
                        Remove-Item $p -Recurse -Force
                    }
            }
            $shortcutRoots = @(
                ""$env:ProgramData\Microsoft\Windows\Start Menu\Programs"",
                ""$env:APPDATA\Microsoft\Windows\Start Menu\Programs"",
                ""$env:PUBLIC\Desktop"",
                [Environment]::GetFolderPath('Desktop')
            )
            $wsh = New-Object -ComObject WScript.Shell
            foreach ($root in $shortcutRoots) {
                if (-not (Test-Path $root)) { continue }
                Get-ChildItem -Path $root -Filter *.lnk -File -Recurse |
                    ForEach-Object {
                        try {
                            $s = $wsh.CreateShortcut($_.FullName)
                            $blob = ""$($s.TargetPath) $($s.Arguments) $($s.IconLocation) $($_.BaseName)""
                            foreach ($pat in '*OUTLOOK.EXE*','*OutlookForWindows*','*Microsoft.Office.Outlook*','*OneDrive.exe*') {
                                if ($blob -like $pat) {
                                    Write-Output ""Removing shortcut $($_.FullName)""
                                    Remove-Item $_.FullName -Force
                                    break
                                }
                            }
                        } catch {}
                    }
            }
            Set-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon' -Name AutoRestartShell -Value 0 -Type DWord -Force
            taskkill /f /im explorer.exe 2>$null | Out-Null
            Start-Sleep -Seconds 2
            $u = (Get-CimInstance Win32_ComputerSystem).UserName
            $shellOk = $false
            if ($u) {
                $a = New-ScheduledTaskAction -Execute 'explorer.exe'
                $pr = New-ScheduledTaskPrincipal -UserId $u -LogonType Interactive -RunLevel Limited
                Register-ScheduledTask -TaskName 'OB_Shell' -Action $a -Principal $pr -Force | Out-Null
                Start-ScheduledTask -TaskName 'OB_Shell'
                Start-Sleep -Seconds 3
                Unregister-ScheduledTask -TaskName 'OB_Shell' -Confirm:$false
                if (Get-Process explorer -ErrorAction SilentlyContinue) { $shellOk = $true }
            }
            Set-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon' -Name AutoRestartShell -Value 1 -Type DWord -Force
            if (-not $shellOk) { Start-Process explorer.exe }
            Write-Output 'OneDrive and new Outlook removal complete'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "oo-remove", ct);
        }
        static Task<int> PrivacyPackAsync(CancellationToken ct) => Task.Run(() =>
        {
            int r = Reg(
                (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "AllowTelemetry", 0),
                (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\DataCollection", "DoNotShowFeedbackNotifications", 1),
                (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\System", "EnableActivityFeed", 0),
                (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\System", "PublishUserActivities", 0),
                (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\System", "UploadUserActivities", 0),
                (HKCU, @"Software\Microsoft\Siuf\Rules", "NumberOfSIUFInPeriod", 0),
                (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Privacy", "TailoredExperiencesWithDiagnosticDataEnabled", 0)
            );
            string[] deny =
            {
                "location", "userAccountInformation", "contacts", "appointments",
                "phoneCallHistory", "email", "userDataTasks", "chat",
                "appDiagnostics", "userNotificationListener",
            };
            foreach (var cat in deny)
            {
                try
                {
                    using var hk = HKCU.CreateSubKey(
                        $@"Software\Microsoft\Windows\CurrentVersion\CapabilityAccessManager\ConsentStore\{cat}", true);
                    hk?.SetValue("Value", "Deny", RegistryValueKind.String);
                }
                catch { }
            }
            return r;
        }, ct);
        static Task<int> ExplorerViewTweaksAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden", 1),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSuperHidden", 1),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_TrackDocs", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton", 0),
            (HKLM, @"SYSTEM\CurrentControlSet\Control\FileSystem", "LongPathsEnabled", 1)
        ), ct);
        static Task<int> ClipboardHistoryOnAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Software\Microsoft\Clipboard", "EnableClipboardHistory", 1),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\System", "AllowClipboardHistory", 1)
        ), ct);
        static Task<int> LongPathsOnAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SYSTEM\CurrentControlSet\Control\FileSystem", "LongPathsEnabled", 1)
        ), ct);

        static Task<int> MouseAccelOffAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Control Panel\Mouse", "MouseSpeed", "0"),
            (HKCU, @"Control Panel\Mouse", "MouseThreshold1", "0"),
            (HKCU, @"Control Panel\Mouse", "MouseThreshold2", "0")
        ), ct);

        static Task<int> GameBarOffAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\GameDVR", "AppCaptureEnabled", 0),
            (HKCU, @"System\GameConfigStore", "GameDVR_Enabled", 0),
            (HKCU, @"System\GameConfigStore", "GameDVR_FSEBehaviorMode", 2),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\GameDVR", "AllowGameDVR", 0),
            (HKCU, @"Software\Microsoft\GameBar", "UseNexusForGameBarEnabled", 0),
            (HKCU, @"Software\Microsoft\GameBar", "ShowStartupPanel", 0)
        ), ct);

        static Task<int> SetDnsCloudflareAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            $dns = @('1.1.1.1','1.0.0.1','2606:4700:4700::1111','2606:4700:4700::1001')
            Get-NetAdapter -Physical | Where-Object { $_.Status -eq 'Up' } | ForEach-Object {
                Write-Output ""Setting DNS $($_.Name)""
                Set-DnsClientServerAddress -InterfaceIndex $_.ifIndex -ServerAddresses $dns
            }
            ipconfig /flushdns | Out-Null
            Write-Output 'DNS set to Cloudflare'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "dns-cf", ct);
        }
        static Task<int> NetBindingsOffAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            foreach ($id in 'ms_pacer','ms_msclient','ms_lltdio','ms_rspndr') {
                Write-Output ""Disabling $id""
                Disable-NetAdapterBinding -Name '*' -ComponentID $id
            }
            Write-Output 'Network bindings trimmed'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "net-bind", ct);
        }
        static Task<int> PowerUltimateAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            $line = powercfg -list | Select-String 'Ultimate Performance'
            if (-not $line) {
                powercfg -duplicatescheme e9a42b02-d5df-448d-aa00-03f14749eb61 | Out-Null
                $line = powercfg -list | Select-String 'Ultimate Performance'
            }
            $g = ([regex]'([0-9a-fA-F-]{36})').Match([string]$line).Value
            if ($g) { powercfg -setactive $g; Write-Output ""Ultimate Performance active ($g)"" }
            else { Write-Output 'Ultimate Performance plan not available' }
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "pwr-ult", ct);
        }

        static Task<int> DisplayMaxRefreshAsync(CancellationToken ct) => Task.Run(() =>
        {
            try
            {
                int n = DisplayConfig.SetMaxRefreshAllMonitors();
                Logger.Info($"Display refresh: updated {n} monitor(s)");
                return 0;
            }
            catch (Exception ex) { Logger.Error("DisplayRefresh", ex); return -1; }
        }, ct);

        static Task<int> DisableDiskEncryptionAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            $bk = 'HKLM:\SYSTEM\CurrentControlSet\Control\BitLocker'
            if (-not (Test-Path $bk)) { New-Item -Path $bk -Force | Out-Null }
            Set-ItemProperty -Path $bk -Name 'PreventDeviceEncryption' -Value 1 -Type DWord -Force
            Get-Volume | Where-Object { $_.DriveLetter } | ForEach-Object {
                $d = ""$($_.DriveLetter):""
                Write-Output ""Decrypting $d (if encrypted)""
                & manage-bde.exe -off $d | Out-Null
            }
            Write-Output 'Disk encryption blocked decrypt requested'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "no-bde", ct);
        }

        static Task<int> DisableUcpdAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            schtasks /Change /Disable /TN '\Microsoft\Windows\AppxDeploymentClient\UCPD velocity' | Out-Null
            Disable-ScheduledTask -TaskPath '\Microsoft\Windows\AppxDeploymentClient\' -TaskName 'UCPD velocity' | Out-Null
            $svc = 'HKLM:\SYSTEM\CurrentControlSet\Services\UCPD'
            if (Test-Path $svc) { Set-ItemProperty -Path $svc -Name 'Start' -Value 4 -Type DWord -Force }
            & sc.exe config UCPD start= disabled | Out-Null
            Write-Output 'UCPD driver velocity disabled'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "ucpd", ct);
        }

        static Task<int> UninstallTerminalAsync(CancellationToken ct)
        {
            const string script = @"
                $ErrorActionPreference='SilentlyContinue'
                foreach ($p in '*WindowsTerminal*','*Microsoft.WindowsTerminal*') {
                    Get-AppxPackage -AllUsers $p | Remove-AppxPackage -AllUsers
                    Get-AppxProvisionedPackage -Online | Where-Object { $_.PackageName -like $p } |
                        ForEach-Object { Remove-AppxProvisionedPackage -Online -AllUsers -PackageName $_.PackageName | Out-Null }
                }
                Write-Output 'Windows Terminal removed'
                ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "rm-term", ct);
        }
        static Task<int> RemoveRdpShortcutsAsync(CancellationToken ct) => Task.Run(() =>
        {
            try
            {
                string[] roots =
                {
                    Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu),
                    Environment.GetFolderPath(Environment.SpecialFolder.StartMenu),
                };
                int removed = 0;
                foreach (var root in roots)
                {
                    if (string.IsNullOrEmpty(root) || !Directory.Exists(root)) continue;
                    foreach (var f in Directory.GetFiles(root, "Remote Desktop Connection*.lnk", SearchOption.AllDirectories))
                    {
                        try { File.Delete(f); removed++; Logger.Info($"Removed RDP shortcut {f}"); } catch { }
                    }
                }
                Logger.Info($"RDP shortcuts removed: {removed}");
                return 0;
            }
            catch (Exception ex) { Logger.Error("RdpShortcuts", ex); return -1; }
        }, ct);

        static Task<int> BingWebSearchOffAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            Get-AppxPackage -AllUsers *BingSearch* | Remove-AppxPackage -AllUsers
            Get-AppxProvisionedPackage -Online | Where-Object { $_.PackageName -like '*BingSearch*' } |
                ForEach-Object { Remove-AppxProvisionedPackage -Online -AllUsers -PackageName $_.PackageName | Out-Null }
            $searchU = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Search'
            if (-not (Test-Path $searchU)) { New-Item -Path $searchU -Force | Out-Null }
            Set-ItemProperty -Path $searchU -Name 'BingSearchEnabled' -Value 0 -Type DWord -Force
            Set-ItemProperty -Path $searchU -Name 'CortanaConsent' -Value 0 -Type DWord -Force
            $expl = 'HKCU:\Software\Policies\Microsoft\Windows\Explorer'
            if (-not (Test-Path $expl)) { New-Item -Path $expl -Force | Out-Null }
            Set-ItemProperty -Path $expl -Name 'DisableSearchBoxSuggestions' -Value 1 -Type DWord -Force
            $wsearch = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\Windows Search'
            if (-not (Test-Path $wsearch)) { New-Item -Path $wsearch -Force | Out-Null }
            Set-ItemProperty -Path $wsearch -Name 'DisableWebSearch' -Value 1 -Type DWord -Force
            Set-ItemProperty -Path $wsearch -Name 'ConnectedSearchUseWeb' -Value 0 -Type DWord -Force
            Set-ItemProperty -Path $wsearch -Name 'AllowCortana' -Value 0 -Type DWord -Force
            $ss = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\SearchSettings'
            if (-not (Test-Path $ss)) { New-Item -Path $ss -Force | Out-Null }
            Set-ItemProperty -Path $ss -Name 'IsAADCloudSearchEnabled' -Value 0 -Type DWord -Force
            Set-ItemProperty -Path $ss -Name 'IsMSACloudSearchEnabled' -Value 0 -Type DWord -Force
            Set-ItemProperty -Path $ss -Name 'IsDeviceSearchHistoryEnabled' -Value 0 -Type DWord -Force
            Set-ItemProperty -Path $ss -Name 'IsDynamicSearchBoxEnabled' -Value 0 -Type DWord -Force
            $fm = 'HKLM:\SYSTEM\CurrentControlSet\Control\FeatureManagement\Overrides\8\1694661260'
            if (-not (Test-Path $fm)) { New-Item -Path $fm -Force | Out-Null }
            Set-ItemProperty -Path $fm -Name 'EnabledState' -Value 1 -Type DWord -Force
            Set-ItemProperty -Path $fm -Name 'EnabledStateOptions' -Value 0 -Type DWord -Force
            Set-ItemProperty -Path $fm -Name 'Variant' -Value 0 -Type DWord -Force
            Set-ItemProperty -Path $fm -Name 'VariantPayload' -Value 0 -Type DWord -Force
            Set-ItemProperty -Path $fm -Name 'VariantPayloadKind' -Value 0 -Type DWord -Force
            Write-Output 'Bing + web + WebView search results disabled'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "no-bing", ct);
        }

        static Task<int> RemoveCapabilitiesPackAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            $remove = @(
                'Browser.InternetExplorer',
                'Hello.Face',
                'MathRecognizer',
                'OpenSSH.Client',
                'App.StepsRecorder',
                'Microsoft.Wallpapers.Extended'
            )
            Get-WindowsCapability -Online | Where-Object { $_.State -eq 'Installed' } | ForEach-Object {
                $name = $_.Name
                foreach ($r in $remove) {
                    if ($name -like ""$r*"") {
                        Write-Output ""Removing capability $name""
                        Remove-WindowsCapability -Online -Name $name | Out-Null
                        break
                    }
                }
            }
            Write-Output 'Optional capabilitie'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "rm-caps", TimeSpan.FromMinutes(15), ct);
        }

        static Task<int> DisableFeaturesPackAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            foreach ($f in 'Printing-Foundation-Features','MSRDC-Infrastructure','SmbDirect','WorkFolders-Client') {
                Write-Output ""Disabling feature $f""
                Disable-WindowsOptionalFeature -Online -FeatureName $f -NoRestart | Out-Null
            }
            Write-Output 'Optional features disabled'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "off-feat", TimeSpan.FromMinutes(15), ct);
        }

        static Task<int> CleanTempCachesAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            # Preserve OrangBooster temp folder and .net temp folder
            $keep = @((Join-Path $env:TEMP 'OrangBooster'), (Join-Path $env:TEMP '.net'))
            Get-ChildItem -LiteralPath $env:TEMP -Force | Where-Object { $keep -notcontains $_.FullName } | Remove-Item -Recurse -Force
            Get-ChildItem -LiteralPath ""$env:SystemRoot\Temp"" -Force | Remove-Item -Recurse -Force
            Remove-Item ""$env:SystemRoot\Prefetch\*"" -Force
            ipconfig /flushdns | Out-Null
            & sc.exe stop wuauserv | Out-Null
            Remove-Item ""$env:SystemRoot\SoftwareDistribution\Download\*"" -Recurse -Force
            & sc.exe start wuauserv | Out-Null
            Remove-Item ""$env:LOCALAPPDATA\Microsoft\Windows\Explorer\thumbcache_*.db"" -Force
            Remove-Item ""$env:LOCALAPPDATA\Microsoft\Windows\Explorer\iconcache_*.db"" -Force
            Write-Output 'Temp caches cleaned'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "clean", ct);
        }

        static Task<int> RestartExplorerAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            Set-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon' -Name AutoRestartShell -Value 0 -Type DWord -Force
            taskkill /f /im explorer.exe 2>$null | Out-Null
            Start-Sleep -Seconds 2
            $u = (Get-CimInstance Win32_ComputerSystem).UserName
            $shellOk = $false
            if ($u) {
                $a = New-ScheduledTaskAction -Execute 'explorer.exe'
                $pr = New-ScheduledTaskPrincipal -UserId $u -LogonType Interactive -RunLevel Limited
                Register-ScheduledTask -TaskName 'OB_Shell' -Action $a -Principal $pr -Force | Out-Null
                Start-ScheduledTask -TaskName 'OB_Shell'
                Start-Sleep -Seconds 3
                Unregister-ScheduledTask -TaskName 'OB_Shell' -Confirm:$false
                if (Get-Process explorer -ErrorAction SilentlyContinue) { $shellOk = $true }
            }
            Set-ItemProperty 'HKLM:\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Winlogon' -Name AutoRestartShell -Value 1 -Type DWord -Force
            if (-not $shellOk) { Start-Process explorer.exe }
            Write-Output 'Explorer restarted'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "restart-expl", ct);
        }

        static Task<int> ClearStartPinsAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            Write-Output 'clearing Start menu pins'
            $pol = 'HKLM:\SOFTWARE\Policies\Microsoft\Windows\Explorer'
            New-Item -Path $pol -Force | Out-Null
            Set-ItemProperty -Path $pol -Name 'ConfigureStartPins' -Value '{""pinnedList"":[]}' -Type String -Force
            Set-ItemProperty -Path $pol -Name 'ConfigureStartPins_ProviderSet' -Value 1 -Type DWord -Force
            $state = ""$env:LOCALAPPDATA\Packages\Microsoft.Windows.StartMenuExperienceHost_cw5n1h2txyewy\LocalState""
            Get-ChildItem -Path $state -Filter 'start*.bin' -ErrorAction SilentlyContinue | ForEach-Object {
                Write-Output ""Removing $($_.FullName)""
                Remove-Item $_.FullName -Force
            }
            $junk = 'Microsoft Edge','WhatsApp','LinkedIn','Instagram','Facebook','Messenger','TikTok','Spotify','Prime Video','ChatGPT','Netflix','Disney'
            $roots = @(
                ""$env:ProgramData\Microsoft\Windows\Start Menu\Programs"",
                ""$env:APPDATA\Microsoft\Windows\Start Menu\Programs""
            )
            foreach ($r in $roots) {
                if (-not (Test-Path $r)) { continue }
                Get-ChildItem -Path $r -Filter *.lnk -File -Recurse |
                    Where-Object { $name = $_.BaseName; $junk | Where-Object { $name -like ""*$_*"" } } |
                    ForEach-Object { Write-Output ""Removing shortcut $($_.FullName)""; Remove-Item $_.FullName -Force }
            }
            Get-Process StartMenuExperienceHost -ErrorAction SilentlyContinue | Stop-Process -Force
            Write-Output '=== Start menu pins cleared ==='
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "clear-start-pins", ct);
        }

        static Task<int> ClearTaskbarPinsAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            Write-Output 'clearing taskbar pins'
            $tb = ""$env:APPDATA\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar""
            if (Test-Path $tb) {
                Get-ChildItem -Path $tb -Filter *.lnk -File | ForEach-Object {
                    Write-Output ""Removing $($_.Name)""
                    Remove-Item $_.FullName -Force
                }
            }
            $band = 'HKCU:\Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband'
            if (Test-Path $band) {
                Remove-ItemProperty -Path $band -Name 'Favorites' -ErrorAction SilentlyContinue
                Remove-ItemProperty -Path $band -Name 'FavoritesResolve' -ErrorAction SilentlyContinue
            }
            Write-Output '=== Taskbar pins cleared (restart explorer to apply) ==='
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "clear-taskbar-pins", ct);
        }

        static Task<int> RemoveStoreAsync(CancellationToken ct)
        {
            const string script = @"
                $ErrorActionPreference='SilentlyContinue'
                foreach ($p in '*Microsoft.WindowsStore*','*Microsoft.StorePurchaseApp*','*Microsoft.Services.Store.Engagement*') {
                    Write-Output ""Removing $p""
                    Get-AppxPackage -AllUsers $p | Remove-AppxPackage -AllUsers
                    Get-AppxProvisionedPackage -Online | Where-Object { $_.PackageName -like $p } |
                        ForEach-Object { Remove-AppxProvisionedPackage -Online -AllUsers -PackageName $_.PackageName | Out-Null }
                }
                Write-Output 'Microsoft Store removed'
                ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "rm-store", ct);
        }

        static Task<int> CopilotSpywareOffAsync(CancellationToken ct)
        {
            const string script = @"
                $ErrorActionPreference='SilentlyContinue'
                Write-Output 'Removing Xbox Game Bar overlay (Game DVR capture)'
                Get-AppxPackage -AllUsers Microsoft.XboxGamingOverlay | Remove-AppxPackage -AllUsers
                Get-AppxProvisionedPackage -Online | Where-Object { $_.PackageName -like '*XboxGamingOverlay*' } |
                    ForEach-Object { Remove-AppxProvisionedPackage -Online -AllUsers -PackageName $_.PackageName | Out-Null }

                $dvr = 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR'
                New-Item -Path $dvr -Force | Out-Null
                Set-ItemProperty -Path $dvr -Name 'AppCaptureEnabled' -Value 0 -Type DWord -Force

                $gcs = 'HKCU:\System\GameConfigStore'
                New-Item -Path $gcs -Force | Out-Null
                Set-ItemProperty -Path $gcs -Name 'GameDVR_Enabled' -Value 0 -Type DWord -Force

                $cop = 'HKCU:\Software\Policies\Microsoft\Windows\WindowsCopilot'
                New-Item -Path $cop -Force | Out-Null
                Set-ItemProperty -Path $cop -Name 'TurnOffWindowsCopilot' -Value 1 -Type DWord -Force

                Write-Output 'Windows Copilot turned off + Game DVR capture disabled'
                ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "copilot-spy", ct);
        }

        static Task<int> LockscreenStartCleanAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SOFTWARE\Policies\Microsoft\Dsh", "DisableWidgetsOnLockScreen", 1),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\PersonalizationCSP", "LockScreenWidgetsEnabled", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", 0)
        ), ct);

        static Task<int> NormalShutdownDialogAsync(CancellationToken ct)
        {
            const string script = @"
                $ErrorActionPreference='SilentlyContinue'
                $k = 'HKLM:\SOFTWARE\Microsoft\WindowsUpdate\Orchestrator'
                New-Item -Path $k -Force | Out-Null
                Remove-ItemProperty -Path $k -Name 'EnhancedShutdownEnabled' -ErrorAction SilentlyContinue
                Set-ItemProperty -Path $k -Name 'ShutdownFlyoutOptions' -Value 0 -Type DWord -Force
                Write-Output 'Shutdown dialog restored to normal'
                ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "shutdown-dialog", ct);
        }

        static Task<int> AccountNagsOffAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSyncProviderNotifications", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Start_AccountNotifications", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\UserProfileEngagement", "ScoobeSystemSettingEnabled", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SoftLandingEnabled", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-338389Enabled", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353694Enabled", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager", "SubscribedContent-353696Enabled", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows Defender Security Center\Account protection", "UILockdown", 1)
        ), ct);

        static Task<int> SettingsHomeOffAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Policies\Explorer", "SettingsPageVisibility", "hide:home"),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer", "SettingsPageVisibility", "hide:home")
        ), ct);

        static Task<int> XboxRemoveFullAsync(CancellationToken ct)
        {
            const string script = @"
                $ErrorActionPreference='SilentlyContinue'
                $pkgs = 'Microsoft.GamingApp','Microsoft.XboxApp','Microsoft.XboxGameOverlay',
                        'Microsoft.XboxGamingOverlay','Microsoft.XboxSpeechToTextOverlay',
                        'Microsoft.Xbox.TCUI','Microsoft.XboxIdentityProvider'
                foreach ($p in $pkgs) {
                    Write-Output ""Removing $p""
                    Get-AppxPackage -AllUsers $p | Remove-AppxPackage -AllUsers
                    Get-AppxProvisionedPackage -Online | Where-Object { $_.PackageName -like ""$p*"" } |
                        ForEach-Object { Remove-AppxProvisionedPackage -Online -AllUsers -PackageName $_.PackageName | Out-Null }
                }
                $rk = 'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\GameDVR'
                if (-not (Test-Path $rk)) { New-Item -Path $rk -Force | Out-Null }
                Set-ItemProperty -Path $rk -Name 'AppCaptureEnabled' -Type DWord -Value 0 -Force
                Write-Output 'Xbox apps removed (GamingServices left intact for Game Pass)'
                ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "rm-xbox", ct);
        }
        static Task<int> Ps7TelemetryOffAsync(CancellationToken ct) => Task.Run(() =>
        {
            try
            {
                Environment.SetEnvironmentVariable("POWERSHELL_TELEMETRY_OPTOUT", "1", EnvironmentVariableTarget.Machine);
                Logger.Info("PowerShell 7 telemetry opt-out env var set");
                return 0;
            }
            catch (Exception ex) { Logger.Error("Ps7TelemetryOff", ex); return -1; }
        }, ct);

        static Task<int> EdgeRemoveFullAsync(CancellationToken ct)
        {
            const string script = @"
                $ErrorActionPreference='SilentlyContinue'
                Write-Output 'Force-removing Microsoft Edge + shortcuts'

                # Stop every Edge
                Get-Process msedge,msedgewebview2,MicrosoftEdgeUpdate,identity_helper,elevation_service -ErrorAction SilentlyContinue | Stop-Process -Force

                # Legacy EdgeHTML appx
                Get-AppxPackage -AllUsers *MicrosoftEdge* | Where-Object { $_.Name -notlike '*WebView*' } | Remove-AppxPackage -AllUsers

                # Forced win32 uninstallation via setup.exe
                $bases = @(""${env:ProgramFiles(x86)}\Microsoft\Edge\Application"", ""$env:ProgramFiles\Microsoft\Edge\Application"")
                foreach ($b in $bases) {
                    if (-not (Test-Path $b)) { continue }
                    Get-ChildItem -Path $b -Directory -ErrorAction SilentlyContinue |
                        Where-Object { $_.Name -match '^[0-9]' } | ForEach-Object {
                            $setup = Join-Path $_.FullName 'Installer\setup.exe'
                            if (Test-Path $setup) {
                                Write-Output ""Uninstalling via $setup""
                                Start-Process -FilePath $setup -ArgumentList '--uninstall','--system-level','--force-uninstall','--msedge' -Wait -WindowStyle Hidden
                            }
                        }
                }

                # Kill EdgeUpdate scheduled tasks
                Get-ScheduledTask -ErrorAction SilentlyContinue | Where-Object { $_.TaskName -like '*Edge*' } |
                    Unregister-ScheduledTask -Confirm:$false
                foreach ($svc in 'edgeupdate','edgeupdatem','MicrosoftEdgeElevationService') {
                    & sc.exe stop $svc | Out-Null
                    & sc.exe delete $svc | Out-Null
                }

                # Block reinstall
                foreach ($k in 'HKLM:\SOFTWARE\Microsoft\EdgeUpdate','HKLM:\SOFTWARE\WOW6432Node\Microsoft\EdgeUpdate') {
                    New-Item -Path $k -Force | Out-Null
                    Set-ItemProperty -Path $k -Name 'DoNotUpdateToEdgeWithChromium' -Value 1 -Type DWord -Force
                }
                $pol = 'HKLM:\SOFTWARE\Policies\Microsoft\EdgeUpdate'
                New-Item -Path $pol -Force | Out-Null
                Set-ItemProperty -Path $pol -Name 'InstallDefault' -Value 0 -Type DWord -Force
                Set-ItemProperty -Path $pol -Name 'Install{56EB18F8-B008-4CBD-B6D2-8C97FE7E9062}' -Value 0 -Type DWord -Force

                # Delete every Edge shortcut
                $dirs = @(
                    ""$env:PUBLIC\Desktop"",
                    ""$env:USERPROFILE\Desktop"",
                    ""$env:ProgramData\Microsoft\Windows\Start Menu\Programs"",
                    ""$env:APPDATA\Microsoft\Windows\Start Menu\Programs"",
                    ""$env:APPDATA\Microsoft\Internet Explorer\Quick Launch"",
                    ""$env:APPDATA\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar"",
                    ""$env:APPDATA\Microsoft\Internet Explorer\Quick Launch\User Pinned\StartMenu""
                )
                foreach ($d in $dirs) {
                    if (-not (Test-Path $d)) { continue }
                    Get-ChildItem -Path $d -Filter '*Edge*.lnk' -File -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
                        Write-Output ""Removing shortcut $($_.FullName)""
                        Remove-Item $_.FullName -Force
                    }
                }

                Write-Output 'Edge removed: binaries, services, tasks, shortcuts + reinstall blocked (WebView2 kept)'
                ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "edge-full", System.TimeSpan.FromMinutes(8), ct);
        }

        static Task<int> XboxServicesOffAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            foreach ($svc in 'XblAuthManager','XblGameSave','XboxGipSvc','XboxNetApiSvc','GamingServices','GamingServicesNet') {
                if (Get-Service -Name $svc -ErrorAction SilentlyContinue) {
                    Write-Output ""Disabling $svc""
                    & sc.exe stop $svc | Out-Null
                    & sc.exe config $svc start= disabled | Out-Null
                }
            }
            Write-Output 'Xbox services disabled'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "xbox-svc", ct);
        }

        static Task<int> MinimizeServicesAsync(CancellationToken ct)
        {
            const string script = @"
            $ErrorActionPreference='SilentlyContinue'
            $svcRoot = 'HKLM:\SYSTEM\CurrentControlSet\Services'
            function Set-SvcStart([string]$name, [int]$start) {
                $k = Join-Path $svcRoot $name
                if (-not (Test-Path $k)) { return }
                try { & sc.exe stop $name | Out-Null } catch {}
                try { Stop-Service -Name $name -Force -ErrorAction SilentlyContinue } catch {}
                Set-ItemProperty -Path $k -Name 'Start' -Value $start -Type DWord -Force
                $word = @{ 4 = 'disabled'; 3 = 'demand'; 2 = 'auto' }[$start]
                & sc.exe config $name start= $word | Out-Null
                Write-Output ""$name -> Start=$start ($word)""
            }
            $disable = 'DiagTrack','CDPSvc','WSearch','SysMain','dmwappushservice','diagnosticshub.standardcollector.service','WerSvc','RetailDemo','MapsBroker','Fax','RemoteRegistry','lfsvc','wisvc','PcaSvc','WMPNetworkSvc','TrkWks'
            foreach ($svc in $disable) { Set-SvcStart $svc 4 }
            foreach ($svc in 'DoSvc','WbioSrvc') { Set-SvcStart $svc 3 }
            Write-Output 'Background services minimized (registry Start values written directly)'
            ";
            return ScriptRunner.RunInlinePowerShellAsync(script, "svc-min", ct);
        }

        static Task<int> UnpinStoreAsync(CancellationToken ct)
        {
            const string script = @"
$ErrorActionPreference='SilentlyContinue'
$inner = @'
$ErrorActionPreference = ""SilentlyContinue""
$apps = (New-Object -ComObject Shell.Application).NameSpace(""shell:AppsFolder"")
$apps.Items() | Where-Object { $_.Name -eq ""Microsoft Store"" -or $_.Path -like ""*Microsoft.WindowsStore*"" } | ForEach-Object {
    $_.Verbs() | Where-Object { ($_.Name -replace ""&"","""") -like ""*npin from taskbar*"" } | ForEach-Object { $_.DoIt() }
}
$tb = ""$env:APPDATA\Microsoft\Internet Explorer\Quick Launch\User Pinned\TaskBar""
if (Test-Path $tb) { Get-ChildItem -Path $tb -Filter ""*Store*.lnk"" -File | Remove-Item -Force }
'@
$path = ""$env:APPDATA\OrangStudio\OrangBooster\ob_unpin_store.ps1""
New-Item -ItemType Directory -Force -Path (Split-Path $path) | Out-Null
Set-Content -LiteralPath $path -Value $inner -Encoding UTF8 -Force
$u = (Get-CimInstance Win32_ComputerSystem).UserName
if ($u) {
    $a = New-ScheduledTaskAction -Execute 'powershell.exe' -Argument ""-NoProfile -ExecutionPolicy Bypass -WindowStyle Hidden -File `""$path`""""
    $pr = New-ScheduledTaskPrincipal -UserId $u -LogonType Interactive -RunLevel Limited
    Register-ScheduledTask -TaskName 'OB_UnpinStore' -Action $a -Principal $pr -Force | Out-Null
    Start-ScheduledTask -TaskName 'OB_UnpinStore'
    Start-Sleep -Seconds 5
    Unregister-ScheduledTask -TaskName 'OB_UnpinStore' -Confirm:$false
}
Remove-Item $path -Force
Write-Output 'Store unpin (interactive user) done'
";
            return ScriptRunner.RunInlinePowerShellAsync(script, "unpin-store", ct);
        }

        static Task<int> AiFullPoliciesAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableAIDataAnalysis", 1),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "AllowRecallEnablement", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableClickToDo", 1),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "TurnOffWindowsCopilot", 1),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableImageCreator", 1),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableGenerativeFill", 1),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\WindowsAI", "DisableCocreator", 1),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableImageCreator", 1),
            (HKLM, @"SOFTWARE\Policies\WindowsNotepad", "DisableAIFeatures", 1),
            (HKCU, @"Software\Policies\Microsoft\Windows\WindowsCopilot", "TurnOffWindowsCopilot", 1),
            (HKCU, @"Software\Microsoft\Windows\Shell\Copilot", "IsCopilotAvailable", 0),
            (HKCU, @"Software\Microsoft\Windows\Shell\Copilot", "CopilotDisabledReason", "FeatureIsDisabled"),
            (HKCU, @"Software\Microsoft\Windows\Shell\Copilot\BingChat", "IsUserEligible", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowCopilotButton", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\M365Copilot", "AutoStartDelayEnabled", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\M365Copilot", "IsCompanionWindowAvailable", 0),
            (HKCU, @"SOFTWARE\Microsoft\Windows\CurrentVersion\WindowsCopilot", "AllowCopilotRuntime", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband\AuxilliaryPins", "CopilotPWAPin", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\Explorer\Taskband\AuxilliaryPins", "RecallPin", 0),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications\Microsoft.Copilot_8wekyb3d8bbwe", "DisabledByUser", 1),
            (HKCU, @"Software\Microsoft\Windows\CurrentVersion\BackgroundAccessApplications\Microsoft.Copilot_8wekyb3d8bbwe", "Disabled", 1),
            (HKCU, @"SOFTWARE\Policies\Microsoft\Windows\CopilotKey", "SetCopilotHardwareKey", " "),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Notifications\Settings", "AutoOpenCopilotLargeScreens", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotPageContext", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotCDPPageContext", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "EdgeEntraCopilotPageContext", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "Microsoft365CopilotChatIconEnabled", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "EdgeHistoryAISearchEnabled", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "GenAILocalFoundationalModelSettings", 1),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "BuiltInAIAPIsEnabled", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "AIGenThemesEnabled", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "ShareBrowsingHistoryWithCopilotSearchAllowed", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "AllowBrowsingWithCopilot", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotNewTabPageEnabled", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "M365LinksAutoOpenCopilotEnabled", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "CopilotAddressBarSuggestionsEnabled", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "HubsSidebarEnabled", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "ComposeInlineEnabled", 0),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Edge", "TabOrganizerSettingEnabled", 0),
            (HKCU, @"Software\Microsoft\Office\16.0\Word\Options", "EnableCopilot", 0),
            (HKCU, @"Software\Microsoft\Office\16.0\Excel\Options", "EnableCopilot", 0),
            (HKCU, @"Software\Microsoft\Office\16.0\PowerPoint\Options", "Enable Copilot in Settings", 0),
            (HKCU, @"Software\Microsoft\Office\16.0\OneNote\Options\Copilot", "CopilotEnabled", 0)
        ), ct);

        static Task<int> GamingPerfPackAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "NetworkThrottlingIndex", unchecked((int)0xFFFFFFFF)),
            (HKLM, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile", "SystemResponsiveness", 0),
            (HKLM, @"SYSTEM\CurrentControlSet\Control\Power\PowerThrottling", "PowerThrottlingOff", 1),
            (HKLM, @"SYSTEM\CurrentControlSet\Control\FileSystem", "NtfsDisableLastAccessUpdate", 1),
            (HKLM, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "GPU Priority", 8),
            (HKLM, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Priority", 6),
            (HKLM, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "Scheduling Category", "High"),
            (HKLM, @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\Multimedia\SystemProfile\Tasks\Games", "SFIO Priority", "High")
        ), ct);

        static Task<int> PaintAiOffAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableCocreator", 1),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableGenerativeFill", 1),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableImageCreator", 1),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableGenerativeErase", 1),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Paint", "DisableRemoveBackground", 1)
        ), ct);

        static Task<int> DeviceCompanionOffAsync(CancellationToken ct) => Task.Run(() => Reg(
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\Device Metadata", "PreventDeviceMetadataFromNetwork", 1),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\Device Metadata", "DeviceMetadataServiceURL", ""),
            (HKLM, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Device Metadata", "PreventDeviceMetadataFromNetwork", 1),
            (HKLM, @"SOFTWARE\Policies\Microsoft\Windows\DeviceInstall\Settings", "DisableSystemRestore", 0)
        ), ct);

        // Display-name wildcards matched against the installed-programs list (Uninstall registry hives).
        public static readonly string[] OemFreewarePatterns =
        {
            // Lenovo
            "Lenovo Vantage*", "Lenovo Settings*", "Lenovo System Interface Foundation*", "Lenovo Account Portal*",
            "Lenovo Service Bridge*", "Lenovo System Update*", "Lenovo Solution Center*", "ThinkVantage*",
            "Lenovo Hotkey*", "Lenovo Power Management Driver*", "Lenovo Utility*", "Lenovo Nerve Center*",
            "Lenovo Mouse Suite*", "*ThinkPad Keyboard Suite*", "Lenovo Ultraslim*", "Lenovo Y Keyboard*",
            "Lenovo Y Gaming*", "Lenovo Le-Note*", "Lenovo Artery*", "Lenovo PiP Anywhere*", "Lenovo Aura*",
            "ThinkPad Stack*", "Lenovo Family Cloud*", "Lenovo QuickCast*", "Lenovo NFC Connector*",
            "Lenovo Migration Assistant*", "Lenovo Photo Master*", "Lenovo App Explorer*", "Yoga Picks*",
            "Lenovo Recommends*", "Lenovo Moto Smart Assistant*", "Lenovo Mobile Assistant*", "Lenovo Bluetooth Lock*",
            "Lenovo Security Console*", "Lenovo Connect*", "Lenovo Battery Gauge*", "Lenovo Companion*",
            "ThinkPad Pen*", "Lenovo QuickControl*", "Lenovo ReachIt*", "Lenovo Quick Optimizer*",
            "ThinkPad Settings Dependency*", "Lenovo ShareIt*", "Lenovo Yoga Camera Man*", "Lenovo Smart Assistant*",
            "Lenovo WriteIt*", "Lenovo Yoga Phone Companion*", "Lenovo Entertainment Hub*", "Lenovo Endpoint Management*",
            "Lenovo Rescue and Recovery*", "Lenovo Access Connections*", "Lenovo Communications Manager*",
            "ThinkPad Power Manager*", "Lenovo Fingerprint*", "Lenovo Password Manager*", "Lenovo AutoLock*",
            "Lenovo Now*", "Lenovo Welcome*", "Lenovo Smart Meeting*", "Lenovo Quick Clean*", "Lenovo Voice*",
            // HP
            "HP Support Assistant*", "OMEN Gaming Hub*", "HP Command Center*", "HP System Event Utility*",
            "HP QuickDrop*", "HP Audio Center*", "HP Enhanced Lighting*", "HP Pen Control*", "HP Palette*",
            "HP Display Control*", "HP Smart*", "HP Easy Start*", "HP Easy Scan*", "HP Print and Scan Doctor*",
            "HP Click*", "HP DesignJet Utility*", "HP ePrint*", "HP Web Jetadmin*", "HP Digital Sending*",
            "HP Image Assistant*", "HP Manageability*", "HP Client Management*", "HP BIOS Configuration Utility*",
            "HP SoftPaq*", "HP System Software Manager*", "HP Cloud Endpoint*", "HP Touchpoint Manager*",
            "HP Wolf Security*", "HP Sure *", "HP Client Security*", "HP Tamper Lock*", "HP Anyware*",
            "HP Performance Advisor*", "HP AI Studio*", "HP Central Web Console*", "HP Remote Graphics*",
            "HP JumpStart*", "HP Documentation*", "HP Connection Optimizer*", "HP Notifications*",
            // Dell / Alienware
            "Dell SupportAssist*", "Alienware*", "Dell Command*", "Dell Mobile Connect*", "Dell Digital Delivery*",
            "Dell Optimizer*", "Dell Display Manager*", "Dell Peripheral Manager*", "Dell Power Manager*",
            "Dell CinemaColor*", "MyDell*", "Dell OpenManage*", "Dell ImageAssist*", "Dell Repository Manager*",
            "Dell Client Command*", "Dell PowerProtect*", "Dell NetWorker*", "Dell Avamar*", "Dell CloudIQ*",
            "Dell SRM*", "Dell APEX*", "Dell Trusted Device*", "Dell Data Guardian*", "Dell Encryption*",
            "Dell Security Management*", "Dell ePSA*", "Dell System E-Support*", "Dell Stage*",
            "Dell Backup and Recovery*", "Dell Webcam Central*", "Dell QuickSet*", "Dell Customer Connect*",
            "Dell Update*", "Dell Core Services*",
            // Samsung
            "Samsung Notes*", "Samsung Gallery*", "Quick Share*", "Samsung Flow*", "Samsung Account*",
            "Multi Control*", "Galaxy Book Experience*", "Second Screen*", "Smart Switch*", "Samsung Settings*",
            "Samsung Update*", "Samsung Device Care*", "Samsung Security*", "Samsung Pass*", "Samsung Recovery*",
            "Samsung Care*", "Samsung Magician*", "Samsung Data Migration*", "Samsung Portable SSD*",
            "SmartThings*", "Samsung Studio*", "Samsung Screen Recorder*", "Studio Plus*", "Samsung TV Plus*",
            "Samsung Pen*", "Screen Cleaner*", "Bixby*", "Galaxy Buds*", "Live Wallpaper*",
            // Security bloat
            "McAfee*", "Norton*", "Avast*", "AVG *",
            // Razer
            "Razer*",
            // LG
            "LG Monitor*", "OnScreen Control*", "LG Switch*", "Dual Controller*", "LG Calibration Studio*",
            "True Color Pro*", "LG Screen Manager*", "UltraGear Control Center*", "LG Update*",
            "LG Smart Assistant*", "LG Glance*", "Glance by Mirametrix*", "LG PC Care*", "LG Troubleshooting*",
            "LG Control Center*", "LG Network Manager*", "LG Power Manager*", "LG ThinQ*", "LG Bridge*",
            "LG Virtoo*", "LG Mobile*", "LG SuperSign*", "LG ConnectedCare*", "LG LED Assistant*", "Smart Share*",
            // Acer
            "PredatorSense*", "NitroSense*", "Acer Care Center*", "Acer Quick Access*", "Acer Purified*",
            "Acer Jumpstart*", "Acer SpatialLabs*", "SpatialLabs Experience*", "Acer LiveGuard*", "Acer Planet9*",
            "Planet9*", "Acer BYOC*", "Acer Photo*", "Acer Media*", "Acer Docs*", "Acer Portal*", "abFiles*",
            "abPhoto*", "Acer Control Center*", "Acer Recovery Management*", "Acer eRecovery*",
            "Acer Office Manager*", "Acer Deployment Tool*", "Acer Product Registration*",
            // MSI
            "MSI Center*", "Dragon Center*", "Creator Center*", "MSI Gaming*", "MSI Dragon*", "MSI Afterburner*",
            "MSI Kombustor*", "MSI True Color*", "MSI Display Kit*", "Mystic Light*", "MSI Smart Tool*",
            "Nahimic*", "MSI App Player*", "MSI Sound Tune*", "Killer Control Center*", "MSI LAN Manager*",
            "MSI Driver Utility*", "MSI BurnRecovery*", "MSI Battery Calibration*", "MSI Help Desk*",
            // ASUS
            "Armoury Crate*", "MyASUS*", "ProArt Creator Hub*", "AI Suite*", "ASUS AI Suite*", "Aura Sync*",
            "GameFirst*", "GameVisual*", "ROG Live Service*", "ROG Armoury*", "Sonic Studio*", "Sonic Radar*",
            "MacroKey*", "ASUS DisplayWidget*", "DisplayWidget*", "ASUS OLED Care*", "ASUS MultiFrame*",
            "ASUS Wi-Fi Master*", "ASUS Smart Gesture*", "ASUS USB 3.0 Boost*", "ASUS WebStorage*",
            "ASUS GiftBox*", "ASUS Product Register*", "ASUS Splendid*", "ASUS ZenLink*",
            // Third-party consumer bundleware
            "Dropbox*", "TikTok*", "Instagram*", "Netflix*", "ExpressVPN*", "LastPass*", "Disney+*",
            "Booking.com*", "Amazon*Assistant*", "WildTangent*", "Keeper Password*",
        };

        // Package-name / full-name wildcards for the UWP sweep.
        public static readonly string[] OemFreewareAppx =
        {
            "*Lenovo*", "E046963F.*", "*ThinkPad*", "*AppExplorer*",
            "AD2F1837.*", "*HPInc*", "*HPSupport*", "*OMEN*", "*HPPrinter*",
            "*Dell*", "*Alienware*", "*SupportAssist*", "PWSDell*",
            "SAMSUNGELECTRONICSCO*", "*Samsung*", "*SmartThings*", "*QuickShare*", "*GalaxyBook*",
            "*McAfee*", "*Norton*", "*Avast*", "*AVGTechnologies*",
            "*Razer*", "*LGElectronics*", "*Acer*", "*MicroStar*", "*MSIGaming*", "*Nahimic*",
            "*ASUS*", "*ArmouryCrate*", "*ProArt*", "*MyASUS*",
            "*Dropbox*", "*TikTok*", "*Instagram*", "*Netflix*", "*ExpressVPN*", "*LastPass*",
            "*Disney*", "*CandyCrush*", "*BubbleWitch*", "*Spotify*",
            "*AdobeSystemsIncorporated*", "*Adobe*",
            "Microsoft.MicrosoftJournal", "*MicrosoftJournal*", "*Recall*", "*ClickToDo*",
            "Microsoft.Surface*", "MicrosoftCorporationII.MicrosoftSurface*", "*SurfaceHub*",
        };

        static string PsArray(string[] items)
            => string.Join(",", System.Array.ConvertAll(items, s => "'" + s.Replace("'", "''") + "'"));

        static Task<int> OemFreewareRemoveAsync(CancellationToken ct)
        {
            string script =
                "$ErrorActionPreference='SilentlyContinue'\n" +
                "$ProgressPreference='SilentlyContinue'\n" +
                "$patterns = @(" + PsArray(OemFreewarePatterns) + ")\n" +
                "$appxPatterns = @(" + PsArray(OemFreewareAppx) + ")\n" +
                OemFreewareBody;
            return ScriptRunner.RunInlinePowerShellAsync(script, "oem-freeware", TimeSpan.FromMinutes(45), ct);
        }

        const string OemFreewareBody = @"
$skip = @('*Driver Package*','*Realtek High Definition Audio Driver*','*Intel(R) Chipset*','*NVIDIA*','*Microsoft Visual C++*','*Windows Driver*')

function Test-Pattern([string]$name, [string[]]$pats) {
    if ([string]::IsNullOrWhiteSpace($name)) { return $false }
    foreach ($p in $pats) { if ($name -like $p) { return $true } }
    return $false
}

function Invoke-Silent([string]$exe, [string]$arguments) {
    try {
        Write-Host ""    exec: $exe $arguments""
        $proc = if ([string]::IsNullOrWhiteSpace($arguments)) {
            Start-Process -FilePath $exe -PassThru -WindowStyle Hidden
        } else {
            Start-Process -FilePath $exe -ArgumentList $arguments -PassThru -WindowStyle Hidden
        }
        if ($null -eq $proc) { return $false }
        if (-not $proc.WaitForExit(600000)) { try { $proc.Kill() } catch { }; Write-Host '    timed out'; return $false }
        Write-Host ""    exit code: $($proc.ExitCode)""
        return ($proc.ExitCode -eq 0 -or $proc.ExitCode -eq 3010 -or $proc.ExitCode -eq 1605 -or $proc.ExitCode -eq 1641)
    } catch { Write-Host ""    failed: $($_.Exception.Message)""; return $false }
}

function Split-UninstallString([string]$s) {
    $s = $s.Trim()
    if ($s.StartsWith('""')) {
        $end = $s.IndexOf('""', 1)
        if ($end -gt 0) { return @($s.Substring(1, $end - 1), $s.Substring($end + 1).Trim()) }
    }
    $m = [regex]::Match($s, '^(.*?\.exe)\s*(.*)$', 'IgnoreCase')
    if ($m.Success) { return @($m.Groups[1].Value.Trim('""'), $m.Groups[2].Value.Trim()) }
    return @($s, '')
}

function Get-SilentArgs([string]$exe, [string]$existing) {
    $leaf = [System.IO.Path]::GetFileName($exe)
    if ($existing -match '(/|-)(S|s)ilent|/qn|/VERYSILENT|/S\b') { return $existing }
    if ($leaf -match '^unins') { return (($existing + ' /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP-').Trim()) }
    if ($leaf -match '^setup') { return (($existing + ' /s /S /qn /norestart').Trim()) }
    return (($existing + ' /S').Trim())
}

$roots = @(
    'HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*',
    'HKLM:\SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\*',
    'HKCU:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\*'
)

$entries = @()
foreach ($r in $roots) { $entries += Get-ItemProperty -Path $r -ErrorAction SilentlyContinue }
Write-Output (""Scanning "" + $entries.Count + "" installed programs against "" + $patterns.Count + "" bloat patterns"")

$targets = $entries | Where-Object {
    $_.DisplayName -and (Test-Pattern $_.DisplayName $patterns) -and -not (Test-Pattern $_.DisplayName $skip) -and -not $_.SystemComponent
} | Sort-Object DisplayName -Unique

if ($targets.Count -eq 0) { Write-Output 'No preloaded freeware matched on this machine' }

foreach ($t in $targets) {
    $name = $t.DisplayName
    Write-Output ""Removing: $name""
    $done = $false
    $code = $t.PSChildName
    $ustr = $t.UninstallString
    $qstr = $t.QuietUninstallString

    $guid = $null
    if ($code -match '^\{[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}\}$') { $guid = $code }
    elseif ($ustr -and $ustr -match '(\{[0-9A-Fa-f]{8}-([0-9A-Fa-f]{4}-){3}[0-9A-Fa-f]{12}\})') { $guid = $Matches[1] }

    if ($guid) {
        Write-Output '    type: MSI'
        $done = Invoke-Silent 'msiexec.exe' ""/x $guid /qn /norestart""
    }
    if (-not $done -and $qstr) {
        Write-Output '    type: quiet uninstall string'
        $parts = Split-UninstallString $qstr
        $done = Invoke-Silent $parts[0] $parts[1]
    }
    if (-not $done -and $ustr) {
        Write-Output '    type: exe uninstaller'
        $parts = Split-UninstallString $ustr
        if (Test-Path -LiteralPath $parts[0]) {
            $done = Invoke-Silent $parts[0] (Get-SilentArgs $parts[0] $parts[1])
        }
    }
    if (-not $done -and (Get-Command winget -ErrorAction SilentlyContinue)) {
        Write-Output '    type: winget fallback'
        try {
            & winget uninstall --name ""$name"" --silent --force --accept-source-agreements --disable-interactivity 2>&1 | Out-Null
            $done = ($LASTEXITCODE -eq 0)
        } catch { }
    }
    if ($done) { Write-Output ""    done: $name"" } else { Write-Output ""    could not silently remove: $name"" }
}

Write-Output 'Sweeping OEM / vendor UWP packages'
$installed = Get-AppxPackage -AllUsers
$prov = Get-AppxProvisionedPackage -Online
foreach ($p in $appxPatterns) {
    $hits = $installed | Where-Object { $_.Name -like $p -or $_.PackageFullName -like $p }
    foreach ($h in $hits) {
        Write-Output ""    appx: $($h.Name)""
        Remove-AppxPackage -Package $h.PackageFullName -AllUsers -ErrorAction SilentlyContinue
    }
    $prov | Where-Object { $_.DisplayName -like $p -or $_.PackageName -like $p } | ForEach-Object {
        Write-Output ""    provisioned: $($_.DisplayName)""
        Remove-AppxProvisionedPackage -Online -PackageName $_.PackageName -AllUsers -ErrorAction SilentlyContinue | Out-Null
    }
}
Write-Output 'Preloaded freeware removal complete'
";
}   }