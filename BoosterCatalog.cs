using System.Collections.Generic;
namespace OrangBooster
{
    public static class BoosterCatalog
    {
        public static IEnumerable<BoosterCard> Build()
        {
            yield return new BoosterCard
            {
                Title = "Disable Telemetry",
                Description = "Block Microsoft's diagnostic data and tailored experiences.",
                Functions = "• Telemetry policy = 0\n• Disable diagtrack + wermgr services\n• Disable tailored experiences and input personalization",
                Tag = CardTag.Privacy,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFTweaksTelemetry" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Activity History",
                Description = "Erases recent docs, clipboard and run history.",
                Functions = "• EnableActivityFeed = 0\n• PublishUserActivities = 0\n• UploadUserActivities = 0",
                Tag = CardTag.Privacy,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFTweaksActivity" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Background Apps",
                Description = "Stop all Microsoft Store apps from running in the background.",
                Functions = "• GlobalUserDisabled = 1\n• Per-user background access blocked",
                Tag = CardTag.Privacy,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFTweaksDisableBGapps" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Location Tracking",
                Description = "Turn off Windows Location Services and per-app location access.",
                Functions = "• lfsvc service disabled\n• Location consent = Deny\n• Sensor permission off",
                Tag = CardTag.Privacy,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFTweaksLocation" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Consumer Features",
                Description = "Stop auto-install of games, third-party apps and Store links.",
                Functions = "• DisableWindowsConsumerFeatures = 1",
                Tag = CardTag.Privacy,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFTweaksConsumerFeatures" },
            };
            yield return new BoosterCard
            {
                Title = "Remove Cortana",
                Description = "Uninstall Cortana UWP and block via policy.",
                Functions = "• Remove-AppxPackage *Cortana*\n• AllowCortana policy = 0\n• Disable web search in search",
                Tag = CardTag.Privacy,
                Recommended = true,
                EmbeddedActions = new[] { "cortana_remove" },
            };
            yield return new BoosterCard
            {
                Title = "Copilot spyware",
                Description = "Turn off Windows Copilot via policy and kill the Xbox Game Bar / Game DVR background capture.",
                Functions = "• Remove Microsoft.XboxGamingOverlay\n• GameDVR AppCaptureEnabled = 0\n• GameConfigStore GameDVR_Enabled = 0\n• WindowsCopilot TurnOffWindowsCopilot = 1",
                Tag = CardTag.Privacy,
                Recommended = true,
                EmbeddedActions = new[] { "copilot_spyware_off" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Lockscreen Widgets & Start Notifications",
                Description = "Removes the lock screen widgets/cards and the notification badges shown on the Start menu account button.",
                Functions = "• Dsh DisableWidgetsOnLockScreen = 1\n• PersonalizationCSP LockScreenWidgetsEnabled = 0\n• Explorer Start_AccountNotifications = 0",
                Tag = CardTag.Privacy,
                Recommended = true,
                EmbeddedActions = new[] { "lockscreen_start_clean" },
            };
            yield return new BoosterCard
            {
                Title = "Remove Microsoft Account Nags",
                Description = "Kills the 'Back up your PC to Microsoft account' toast, the account ads in Explorer and Start, the 'finish setting up' prompts, and the Account protection nag in Windows Security.",
                Functions = "• ShowSyncProviderNotifications = 0\n• Start_AccountNotifications = 0\n• ScoobeSystemSettingEnabled = 0\n• ContentDeliveryManager suggestions off\n• Defender Account protection UILockdown = 1",
                Tag = CardTag.Privacy,
                Recommended = true,
                EmbeddedActions = new[] { "account_nags_off" },
            };
            yield return new BoosterCard
            {
                Title = "Remove Settings Home Page",
                Description = "Hides the Home page from the Settings app so it opens straight to System.",
                Functions = "• Policies\\Explorer SettingsPageVisibility - hide:home (HKCU + HKLM)",
                Tag = CardTag.Customizable,
                Recommended = true,
                EmbeddedActions = new[] { "settings_home_off" },
            };
            yield return new BoosterCard
            {
                Title = "Normal Shutdown dialog",
                Description = "Restores the classic shutdown dialog by removing Windows Update's enhanced shutdown options.",
                Functions = "• Delete Orchestrator\\EnhancedShutdownEnabled\n• ShutdownFlyoutOptions = 0",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "normal_shutdown_dialog" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Bing in Start Search",
                Description = "Remove Bing web/Copilot results from the Start menu search, and strip the WebView/web-results panel from Search entirely.",
                Functions = "• Win11Debloat -DisableBing\n• BingSearchEnabled = 0, DisableSearchBoxSuggestions = 1\n• DisableWebSearch / ConnectedSearchUseWeb off\n• SearchSettings cloud + device history off\n• FeatureManagement override 8\\1694661260 (kills the WebView web results)",
                Tag = CardTag.Privacy,
                Recommended = true,
                Win11DebloatArgs = new[] { "-DisableBing" },
                EmbeddedActions = new[] { "bing_websearch_off" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Advertising ID",
                Description = "Block the advertising ID and Content Delivery Manager suggestions.",
                Functions = "• AdvertisingInfo Enabled = 0\n• Tailored experiences off\n• Lockscreen / Start suggestions off",
                Tag = CardTag.Privacy,
                Recommended = true,
                EmbeddedActions = new[] { "advertising_id_off" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Notifications & Calendar",
                Description = "Silences toast notifications and tray notification center.",
                Functions = "• DisableNotificationCenter = 1\n• ToastEnabled = 0",
                Tag = CardTag.Privacy,
                WinUtilTweaks = new[] { "WPFTweaksDisableNotifications" },
            };
            yield return new BoosterCard
            {
                Title = "Block Adobe Telemetry Hosts",
                Description = "Add Ruddernation Adobe URL block list to your hosts file.",
                Functions = "• Backup current hosts file\n• Append Adobe activation/telemetry blocklist\n• Flush DNS",
                Tag = CardTag.Privacy,
                WinUtilTweaks = new[] { "WPFTweaksBlockAdobeNet" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Hibernation",
                Description = "Frees disk space (removes hiberfil.sys) and disables hibernate.",
                Functions = "• powercfg /hibernate off\n• HibernateEnabled = 0\n• Hide hibernate option in shutdown menu",
                Tag = CardTag.Safe,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFTweaksHiber" },
            };
            yield return new BoosterCard
            {
                Title = "Trim Services",
                Description = "Set unused services to Manual or Disabled; tune svchost split threshold.",
                Functions = "• DiagTrack, CscService, SharedAccess → Disabled\n• MapsBroker, StorSvc → Manual\n• SvcHostSplitThresholdInKB = RAM size",
                Tag = CardTag.Safe,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFTweaksServices" },
            };
            yield return new BoosterCard
            {
                Title = "Disable WPBT",
                Description = "Block vendors from executing code at boot via Windows Platform Binary Table.",
                Functions = "• DisableWpbtExecution = 1",
                Tag = CardTag.Safe,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFTweaksWPBT" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Storage Sense",
                Description = "Stops Windows from automatically deleting temp files and downloads.",
                Functions = "• StorageSense StoragePolicy 01 = 0",
                Tag = CardTag.Safe,
                WinUtilTweaks = new[] { "WPFTweaksStorage" },
            };
            yield return new BoosterCard
            {
                Title = "End Task on Taskbar",
                Description = "Adds the 'End task' option when you right-click a taskbar app.",
                Functions = "• TaskbarEndTask = 1",
                Tag = CardTag.Safe,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFTweaksEndTaskOnTaskbar" },
            };
            yield return new BoosterCard
            {
                Title = "Disable PowerShell 7 Telemetry",
                Description = "Sets POWERSHELL_TELEMETRY_OPTOUT=1 system-wide.",
                Functions = "• Machine-scope env var written",
                Tag = CardTag.Safe,
                EmbeddedActions = new[] { "ps7_telemetry_off" },
            };
            yield return new BoosterCard
            {
                Title = "Block Razer Auto Install",
                Description = "Prevents Windows from auto-installing Razer Synapse bloat on connect.",
                Functions = "• Driver search order tightened\n• Razer installer folder locked",
                Tag = CardTag.Safe,
                WinUtilTweaks = new[] { "WPFTweaksRazerBlock" },
            };
            yield return new BoosterCard
            {
                Title = "Enable Long Paths",
                Description = "Allow file paths longer than 260 characters.",
                Functions = "• LongPathsEnabled = 1",
                Tag = CardTag.Safe,
                Recommended = true,
                EmbeddedActions = new[] { "long_paths_on" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Sticky Toggle Filter Keys",
                Description = "Kills the accessibility key shortcuts (5x shift etc.) for all four families.",
                Functions = "• StickyKeys Flags = 506\n• ToggleKeys Flags = 58\n• FilterKeys Flags = 34\n• Keyboard Response Flags = 122",
                Tag = CardTag.Safe,
                Recommended = true,
                EmbeddedActions = new[] { "all_a11y_off" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Fullscreen Optimizations",
                Description = "Disables FSO globally for fewer compositor stalls in games.",
                Functions = "• GameDVR_DXGIHonorFSEWindowsCompatible = 1",
                Tag = CardTag.Safe,
                WinUtilTweaks = new[] { "WPFTweaksDisableFSO" },
            };
            yield return new BoosterCard
            {
                Title = "Disable BitLocker",
                Description = "Decrypts all volumes and blocks auto device encryption.",
                Functions = "• PreventDeviceEncryption = 1\n• manage-bde -off on every volume",
                Tag = CardTag.Safe,
                EmbeddedActions = new[] { "disable_disk_encryption" },
            };
            yield return new BoosterCard
            {
                Title = "Snappy Window Timeouts",
                Description = "Shorter app-hang timeouts and zero menu show delay.",
                Functions = "• MenuShowDelay = 0\n• WaitToKillAppTimeout = 2000\n• HungAppTimeout = 1000\n• AutoEndTasks = 1",
                Tag = CardTag.Safe,
                Recommended = true,
                EmbeddedActions = new[] { "perf_responsiveness" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Reserved Storage",
                Description = "Reclaims the ~7 GB Windows reserves for updates.",
                Functions = "• Set-WindowsReservedStorageState Disabled",
                Tag = CardTag.Safe,
                EmbeddedActions = new[] { "reserved_storage_off" },
            };

            yield return new BoosterCard
            {
                Title = "Disable IPv6",
                Description = "Disables IPv6 stack-wide. Breaks some home network features.",
                Functions = "• Disable-NetAdapterBinding ms_tcpip6\n• Tcpip6 DisabledComponents = 255",
                Tag = CardTag.Unsafe,
                WinUtilTweaks = new[] { "WPFTweaksDisableIPv6" },
            };
            yield return new BoosterCard
            {
                Title = "Prefer IPv4 over IPv6",
                Description = "Keep IPv6 enabled but prefer IPv4 for resolution.",
                Functions = "• Tcpip6 DisabledComponents = 32",
                Tag = CardTag.Unsafe,
                WinUtilTweaks = new[] { "WPFTweaksIPv46" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Teredo",
                Description = "Removes the IPv6 transition tunnel that adds latency.",
                Functions = "• netsh teredo state disabled\n• DisabledComponents bit set",
                Tag = CardTag.Unsafe,
                WinUtilTweaks = new[] { "WPFTweaksTeredo" },
            };
            yield return new BoosterCard
            {
                Title = "TCP / Nagle Optimizations",
                Description = "Walks every network interface and disables Nagle's algorithm.",
                Functions = "• Per-interface TCPNoDelay = 1\n• TcpAckFrequency = 1\n• TcpDelAckTicks = 0\n• IRPStackSize = 30",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "tcp_nagle" },
            };
            yield return new BoosterCard
            {
                Title = "Network Stack Reset",
                Description = "Fresh-start your TCP/IP stack and tune global TCP params.",
                Functions = "• netsh winsock reset\n• autotuninglevel disabled\n• dca + ecn + timestamps enabled\n• flush + register DNS",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "net_stack_reset" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Windows ai slop",
                Description = "Removes Copilot, disables Recall and Click To Do, hides AI components, and applies the full RemoveWindowsAI policy set (Edge AI, Office Copilot, taskband pins, Copilot key).",
                Functions = "• Robust embedded Copilot/Recall nuke (appx + provisioned + winget + policies)\n• Full RemoveWindowsAI reg policy pass: WindowsAI/Copilot/Recall, Edge AI, Office Copilot, M365Copilot, taskband pins, Copilot hardware key\n• Paint AI off: Cocreator, Generative Fill/Erase, Image Creator, Remove Background\n• WPFTweaksWindowsAI\n• Win11Debloat -DisableCopilot -DisableRecall -DisableClickToDo -DisableAISvcAutoStart",
                Tag = CardTag.Unsafe,
                Recommended = true,
                EmbeddedActions = new[] { "nuke_ai_copilot", "ai_full_policies", "paint_ai_off" },
                WinUtilTweaks = new[] { "WPFTweaksWindowsAI" },
                Win11DebloatArgs = new[] { "-DisableCopilot", "-DisableRecall", "-DisableClickToDo", "-DisableAISvcAutoStart" },
            };
            yield return new BoosterCard
            {
                Title = "Remove Xbox & Game Bar",
                Description = "Fully uninstalls the Xbox app, Game Bar, overlays, DVR and related services.",
                Functions = "• Remove-AppxPackage GamingApp/XboxApp/overlays/TCUI/IdentityProvider\n• GameDVR AppCaptureEnabled = 0\n• Win11Debloat -DisableDVR -DisableGameBarIntegration\n• GamingServices kept (Game Pass)",
                Tag = CardTag.Unsafe,
                Recommended = true,
                Win11DebloatArgs = new[] { "-DisableDVR", "-DisableGameBarIntegration" },
                EmbeddedActions = new[] { "xbox_remove_full" },
            };
            yield return new BoosterCard
            {
                Title = "Uninstall All Bloatware Apps",
                Description = "Same aggressive appx sweep as the Debloater - removes 100+ preinstalled bloat apps for the current and provisioned users.",
                Functions = "• Get-AppxPackage -AllUsers | Remove-AppxPackage\n• Remove-AppxProvisionedPackage for each\n• Uses the OrangBooster bloatware list (Bing apps, Solitaire, Teams, Xbox, Clipchamp, etc.)",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "appx_bloat_remove" },
            };
            yield return new BoosterCard
            {
                Title = "Strip Windows Optional Features",
                Description = "Disables Media Player, XPS, Print-to-PDF service, Work Folders and Recall.",
                Functions = "• dism /disable-feature for each",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "dism_strip_features" },
            };
            yield return new BoosterCard
            {
                Title = "Ultimate Performance Power Plan",
                Description = "Installs the hidden Ultimate Performance plan and sets it as the current/active plan.",
                Functions = "• powercfg /duplicatescheme + powercfg -setactive\n• IDLEDISABLE = 1\n• PROCTHROTTLEMIN = 100",
                Tag = CardTag.Unsafe,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFAddUltPerf" },
                EmbeddedActions = new[] { "power_ultimate" },
            };
            yield return new BoosterCard
            {
                Title = "Updates: Security Only home",
                Description = "Defer feature updates 365 days, security only. Home edition friendly.",
                Functions = "• HKLM Policies\\WindowsUpdate keys\n• TargetReleaseVersion = 24H2\n• DeferQualityUpdates = 4 days",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "raven_update_policy" },
            };
            yield return new BoosterCard
            {
                Title = "Updates: Security Only Entr/Pro",
                Description = "Permanently blocks feature updates + driver updates on Pro/Enterprise.",
                Functions = "• ExcludeUpdateClassifications (drivers + features)\n• AUOptions = 2\n• Restart wuauserv",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "raven_update_policy_pro" },
            };
            yield return new BoosterCard
            {
                Title = "Uninstall OneDrive & New Outlook",
                Description = "Talon-style sweep. Path-fixed for modern OneDrive (per-user Update folder) and handles both new + legacy Outlook UWP names.",
                Functions = "• Kill OneDrive / Outlook procs\n• Try setup.exe in System32, SysWOW64, %LOCALAPPDATA%\\…\\Update\n• Winget fallback\n• Wipe folders + namespace CLSIDs\n• Remove appx (Microsoft.OutlookForWindows + Microsoft.Office.Outlook)\n• takeown + ACL strip WindowsApps folder\n• Restart Explorer",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "oo_remove_robust" },
            };

            yield return new BoosterCard
            {
                Title = "Visual Effects Performance",
                Description = "Equivalent to 'Adjust for best performance' in Sysdm.",
                Functions = "• DragFullWindows, MinAnimate, TaskbarAnimations off\n• VisualFXSetting = 3\n• UserPreferencesMask binary",
                Tag = CardTag.Customizable,
                WinUtilTweaks = new[] { "WPFTweaksDisplay" },
            };
            yield return new BoosterCard
            {
                Title = "Classic Right-Click Menu",
                Description = "Restores the Windows 10 context menu in Explorer.",
                Functions = "• HKCU CLSID InprocServer32 shim\n• Restart explorer",
                Tag = CardTag.Customizable,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFTweaksRightClickMenu" },
            };
            yield return new BoosterCard
            {
                Title = "Old Start Menu Layout",
                Description = "Reverts the 25H2 Start menu rollout via ViVeTool.",
                Functions = "• Download ViVeTool\n• /disable /id:47205210",
                Tag = CardTag.Customizable,
                WinUtilTweaks = new[] { "WPFTweaksRevertStartMenu" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Explorer Auto Folder Discovery",
                Description = "Stops Explorer from guessing folder types (slow browsing).",
                Functions = "• Flush Bags + BagMRU\n• AllFolders FolderType = NotSpecified",
                Tag = CardTag.Customizable,
                WinUtilTweaks = new[] { "WPFTweaksDisableExplorerAutoDiscovery" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Microsoft Store Search Suggestions",
                Description = "No more 'Get from the Store' results in Start search.",
                Functions = "• icacls deny on store.db",
                Tag = CardTag.Customizable,
                WinUtilTweaks = new[] { "WPFTweaksDisableStoreSearch" },
            };
            yield return new BoosterCard
            {
                Title = "Show File Extensions",
                Description = "Display file extensions for known types.",
                Functions = "• HideFileExt = 0",
                Tag = CardTag.Customizable,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFToggleShowExt" },
            };
            yield return new BoosterCard
            {
                Title = "Show Hidden Files",
                Description = "Toggle hidden files visible in Explorer.",
                Functions = "• Hidden = 1",
                Tag = CardTag.Customizable,
                WinUtilTweaks = new[] { "WPFToggleHiddenFiles" },
            };
            yield return new BoosterCard
            {
                Title = "Dark Mode",
                Description = "Switch Windows to the default dark theme (system + apps), like the debloater does.",
                Functions = "• Apply default dark.theme file\n• AppsUseLightTheme = 0\n• SystemUsesLightTheme = 0\n• Disable lock-screen Spotlight ads",
                Tag = CardTag.Customizable,
                Recommended = true,
                EmbeddedActions = new[] { "force_dark_no_spotlight" },
                WinUtilTweaks = new[] { "WPFToggleDarkMode" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Mouse Acceleration",
                Description = "Turns off Enhance Pointer Precision.",
                Functions = "• MouseSpeed = 0\n• MouseThreshold1/2 = 0",
                Tag = CardTag.Customizable,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFToggleMouseAcceleration" },
            };
            yield return new BoosterCard
            {
                Title = "Taskbar Align Left",
                Description = "Windows 10-style left-aligned taskbar icons.",
                Functions = "• TaskbarAl = 0\n• Restart explorer",
                Tag = CardTag.Customizable,
                Recommended = true,
                WinUtilTweaks = new[] { "WPFToggleTaskbarAlignment" },
            };
            yield return new BoosterCard
            {
                Title = "Verbose BSoD",
                Description = "Show detailed BSoD parameters instead of the emoji screen.",
                Functions = "• DisplayParameters = 1\n• DisableEmoticon = 1",
                Tag = CardTag.Customizable,
                WinUtilTweaks = new[] { "WPFToggleDetailedBSoD" },
            };
            yield return new BoosterCard
            {
                Title = "Hide Explorer Home & Gallery",
                Description = "Remove Home + Gallery from the navigation pane, open to This PC.",
                Functions = "• HubMode = 1\n• Delete Home + Gallery NameSpace keys\n• LaunchTo = 1",
                Tag = CardTag.Customizable,
                EmbeddedActions = new[] { "explorer_home_gallery_off" },
            };
            yield return new BoosterCard
            {
                Title = "OrangBooster UI Performance Pack",
                Description = "Registry pack: small UX wins, long paths, verbose BSoD, hide TaskView, etc.",
                Functions = "• Bundle of QoL HKCU/HKLM tweaks\n• Doesn't fight your other selections",
                Tag = CardTag.Customizable,
                EmbeddedActions = new[] { "ob_registry_pack" },
            };

            yield return new BoosterCard
            {
                Title = "24-Hour Clock, not that AM/PM",
                Description = "Force 24-hour time format in the taskbar / locale.",
                Functions = "• sShortTime = HH:mm\n• sTimeFormat = HH:mm:ss",
                Tag = CardTag.Customizable,
                EmbeddedActions = new[] { "clock_24h" },
            };
            yield return new BoosterCard
            {
                Title = "Show Seconds in Clock",
                Description = "Display seconds in the system tray clock.",
                Functions = "• ShowSecondsInSystemClock = 1",
                Tag = CardTag.Customizable,
                EmbeddedActions = new[] { "clock_seconds" },
            };
            yield return new BoosterCard
            {
                Title = "Hardware-Accelerated GPU Scheduling",
                Description = "Enables HAGS = can lower latency on modern GPUs. Requires reboot.",
                Functions = "• HwSchMode = 2 (HKLM\\SYSTEM\\...\\GraphicsDrivers)",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "enable_hags" },
            };
            yield return new BoosterCard
            {
                Title = "Enable Game Mode",
                Description = "Turns on Windows Game Mode + auto game detection.",
                Functions = "• AllowAutoGameMode = 1\n• AutoGameModeEnabled = 1",
                Tag = CardTag.Safe,
                Recommended = true,
                EmbeddedActions = new[] { "enable_game_mode" },
            };
            yield return new BoosterCard
            {
                Title = "DirectX Windowed Game Optimization",
                Description = "Enables MSFT swap-effect upgrade for windowed/borderless games (Auto HDR / VRR friendly).",
                Functions = "• DirectXUserGlobalSettings = SwapEffectUpgradeEnable=1",
                Tag = CardTag.Customizable,
                EmbeddedActions = new[] { "dx_windowed_opt" },
            };
            yield return new BoosterCard
            {
                Title = "Gaming & Latency Pack",
                Description = "CFixer/old-Orange latency tweaks: kill network + power throttling, MMCSS gaming priority, and stop NTFS last-access writes.",
                Functions = "• NetworkThrottlingIndex = 0xFFFFFFFF\n• SystemResponsiveness = 0\n• PowerThrottlingOff = 1\n• MMCSS Games: GPU Priority 8, Priority 6, Scheduling/SFIO = High\n• NtfsDisableLastAccessUpdate = 1",
                Tag = CardTag.Unsafe,
                Recommended = true,
                EmbeddedActions = new[] { "gaming_perf_pack" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Core Isolation (Memory Integrity)",
                Description = "Turns off HVCI. Major FPS win on some systems. Reduces hypervisor-level protection.",
                Functions = "• HVCI Enabled = 0",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "disable_core_isolation" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Dynamic Ticking",
                Description = "bcdedit /set disabledynamictick yes - can smooth out timer-sensitive workloads.",
                Functions = "• bcdedit /set disabledynamictick yes",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "disable_dynamic_ticking" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Fast Startup",
                Description = "Forces a true cold shutdown each time (avoids hybrid hiberfile boot).",
                Functions = "• HiberbootEnabled = 0",
                Tag = CardTag.Safe,
                Recommended = true,
                EmbeddedActions = new[] { "disable_fast_startup" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Wi-Fi Sense",
                Description = "Stops Windows from auto-connecting to suggested open hotspots and sharing creds.",
                Functions = "• AllowWiFiHotSpotReporting = 0\n• AllowAutoConnectToWiFiSenseHotspots = 0",
                Tag = CardTag.Privacy,
                Recommended = true,
                EmbeddedActions = new[] { "disable_wifi_sense" },
            };
            yield return new BoosterCard
            {
                Title = "RTC: Set Time to UTC",
                Description = "Stores the hardware clock in UTC. Pairs with Linux dual-boot.",
                Functions = "• RealTimeIsUniversal = 1",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "set_time_utc" },
            };
            yield return new BoosterCard
            {
                Title = "CPU Priority: Foreground Boost",
                Description = "Win32PrioritySeparation = 36 = foreground apps get bigger time slices.",
                Functions = "• Win32PrioritySeparation = 36 (HKLM\\PriorityControl)",
                Tag = CardTag.Safe,
                Recommended = true,
                EmbeddedActions = new[] { "priority_separation_fg" },
            };
            yield return new BoosterCard
            {
                Title = "Net Tweaks",
                Description = "netsh int tcp: heuristics off, RSS, ECN, fast open, congestion=ctcp, ICW 10. Thanks sparkle project",
                Functions = "• heuristics disabled\n• rss enabled\n• ecn + fastopen on\n• supplemental icw=10",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "net_sparkle_tweaks" },
            };
            yield return new BoosterCard
            {
                Title = "Disable RDP Unsigned File Warnings",
                Description = "Stops the modal when launching .rdp files. Slight phishing risk if you click random .rdp.",
                Functions = "• RedirectionWarningDialogVersion = 1",
                Tag = CardTag.Customizable,
                EmbeddedActions = new[] { "disable_rdp_warnings" },
            };
            yield return new BoosterCard
            {
                Title = "Remove Widgets + Hide Task View",
                Description = "Kills the Widgets WebExperience package and hides Task View from the taskbar.",
                Functions = "• Remove-AppxPackage *WebExperience*\n• ShowTaskViewButton = 0\n• Restart explorer",
                Tag = CardTag.Customizable,
                Recommended = true,
                EmbeddedActions = new[] { "disable_widgets_taskview" },
            };
            yield return new BoosterCard
            {
                Title = "Edge Remover",
                Description = "Runs the AveYo's edge.bat, then force-uninstalls Edge, deletes every Edge shortcut, kills the EdgeUpdate tasks/services and blocks reinstall. WebView2 stays.",
                Functions = "• Bundled edge.bat invoked silently\n• setup.exe --uninstall --force-uninstall --msedge\n• Delete Edge shortcuts (desktop / Start / taskbar)\n• Remove EdgeUpdate tasks + services\n• EdgeUpdate InstallDefault = 0 (blocks reinstall)",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "edge_remover_bat", "edge_remove_full" },
            };
            yield return new BoosterCard
            {
                Title = "Remove Windows Terminal",
                Description = "Uninstalls Windows Terminal for all users and de-provisions it.",
                Functions = "• Remove-AppxPackage *WindowsTerminal* (all users)\n• Remove-AppxProvisionedPackage",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "uninstall_terminal" },
            };
            yield return new BoosterCard
            {
                Title = "Remove Microsoft Store",
                Description = "Uninstalls the Microsoft Store app and its helpers. Breaks Store app updates and some winget sources.",
                Functions = "• Remove-AppxPackage *Microsoft.WindowsStore* (all users)\n• Remove StorePurchaseApp + Store.Engagement\n• Remove-AppxProvisionedPackage",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "remove_store" },
            };
            yield return new BoosterCard
            {
                Title = "Clear Start & Taskbar Pins",
                Description = "Wipes pinned tiles from the Start menu and removes every pinned app from the taskbar, like the debloater does.",
                Functions = "• ConfigureStartPins policy - empty list\n• Wipe start*.bin + bloat shortcuts\n• Clear Taskband Favorites + Quick Launch pins\n• Restart explorer",
                Tag = CardTag.Customizable,
                Recommended = true,
                EmbeddedActions = new[] { "clear_start_pins", "clear_taskbar_pins", "restart_explorer" },
            };
            yield return new BoosterCard
            {
                Title = "Trim Network Adapter Bindings",
                Description = "Unchecks the unneeded protocols/services on every network adapter, like the debloater's Network Connections cleanup.",
                Functions = "• Disable-NetAdapterBinding ms_pacer (QoS Scheduler)\n• ms_msclient (Client for MS Networks)\n• ms_lltdio + ms_rspndr (LLDP mapper)",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "net_bindings_off" },
            };
            yield return new BoosterCard
            {
                Title = "Remove Paint AI",
                Description = "Kills the AI features baked into Paint - Cocreator, Generative Fill/Erase, Image Creator and Remove Background.",
                Functions = "• Policies\\Paint DisableCocreator = 1\n• DisableGenerativeFill = 1\n• DisableImageCreator = 1\n• DisableGenerativeErase = 1\n• DisableRemoveBackground = 1",
                Tag = CardTag.Privacy,
                Recommended = true,
                EmbeddedActions = new[] { "paint_ai_off" },
            };
            yield return new BoosterCard
            {
                Title = "Disable Device Companion Apps",
                Description = "Stops Windows from pulling device metadata off the network, which is what triggers the automatic install of vendor companion apps when you plug hardware in.",
                Functions = "• Device Metadata PreventDeviceMetadataFromNetwork = 1\n• DeviceMetadataServiceURL cleared\n• Applied under both Policies and CurrentVersion",
                Tag = CardTag.Privacy,
                Recommended = true,
                EmbeddedActions = new[] { "device_companion_off" },
            };
            yield return new BoosterCard
            {
                Title = "Remove preloaded freeware",
                Description = "VERY DANGEROUS. Scans your installed programs and silently uninstalls OEM/vendor preloads - Lenovo, HP, Dell, Samsung, Acer, MSI, ASUS, LG, Razer plus McAfee/Norton/Avast and bundled consumer apps. Removes MSI, EXE and UWP installs alike. Read the list before running: it will take vendor utilities and drivers like Lenovo Power Management with it.",
                Functions = "• Enumerate HKLM/HKLM-WOW64/HKCU Uninstall hives\n• Wildcard match against the OrangBooster OEM bloat list\n• MSI products → msiexec /x {GUID} /qn /norestart\n• QuietUninstallString used when present\n• EXE uninstallers → Inno /VERYSILENT, NSIS /S, setup /s /qn detection\n• winget uninstall --silent --force fallback\n• UWP sweep: Remove-AppxPackage -AllUsers + Remove-AppxProvisionedPackage (vendor, Adobe, Journal, Recall, Surface, Candy Crush, TikTok, Netflix, Dropbox…)",
                Tag = CardTag.Unsafe,
                EmbeddedActions = new[] { "oem_freeware_remove" },
            };
}   }   }