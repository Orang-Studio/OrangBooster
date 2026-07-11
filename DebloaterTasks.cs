using System.Threading;
using System.Threading.Tasks;

namespace OrangBooster
{
    public enum DebloaterId { CttTasks, RaphiDebloat, AppUninstaller, EdgeRemover, OrangBoosterTasks }

    public static class DebloaterTasks
    {
        public static readonly string[] DefaultWinUtilTweaks =
        {
            "WPFTweaksActivity", "WPFTweaksTelemetry", "WPFTweaksLocation",
            "WPFTweaksDisableBGapps", "WPFTweaksConsumerFeatures", "WPFTweaksDisableNotifications",
            "WPFTweaksWPBT",
            "WPFTweaksDeBloat", "WPFTweaksServices", "WPFTweaksDisableStoreSearch",
            "WPFTweaksRemoveOneDrive", "WPFTweaksRemoveEdge",
            "WPFTweaksWidget", "WPFTweaksXboxRemoval", "WPFTweaksWindowsAI",
            "WPFTweaksDisableExplorerAutoDiscovery", "WPFTweaksRevertStartMenu",
            "WPFTweaksRightClickMenu", "WPFTweaksRemoveHome",
            "WPFTweaksEndTaskOnTaskbar", "WPFTweaksDisplay",
            "WPFTweaksHiber", "WPFTweaksEdgeDebloat",
            "WPFToggleDarkMode", "WPFToggleShowExt", "WPFToggleTaskbarAlignment",
            "WPFToggleStartMenuRecommendations",
            "WPFToggleHideSettingsHome", "WPFToggleStickyKeys",
            "WPFToggleBingSearch", "WPFToggleNewOutlook",
            "WPFToggleMouseAcceleration",
        };

        public static readonly string[] DefaultWin11DebloatArgs =
        {
            "-Silent",
            "-RemoveApps", "-RemoveGamingApps", "-RemoveCommApps", "-RemoveW11Outlook",
            "-DisableTelemetry", "-DisableSuggestions", "-DisableLockscreenTips",
            "-DisableDesktopSpotlight", "-DisableSettings365Ads", "-DisableSettingsHome",
            "-DisableSearchHistory", "-DisableSearchHighlights", "-DisableStoreSearchSuggestions",
            "-DisableEdgeAds",
            "-DisableBing", "-DisableCopilot", "-DisableRecall", "-DisableClickToDo",
            "-DisableAISvcAutoStart", "-DisableEdgeAI", "-DisablePaintAI", "-DisableNotepadAI",
            "-TaskbarAlignLeft", "-HideSearchTb", "-HideTaskview", "-HideChat",
            "-DisableWidgets", "-ClearStartAllUsers", "-DisableStartRecommended",
            "-EnableEndTask", "-EnableLastActiveClick",
            "-ExplorerToThisPC", "-RevertContextMenu", "-ShowKnownFileExt", "-ShowHiddenFolders",
            "-HideHome", "-HideGallery", "-HideDupliDrive",
            "-DisableMouseAcceleration", "-DisableStickyKeys", "-DisableGameBarIntegration",
            "-DisableDVR", "-DisableFastStartup", "-DisableModernStandbyNetworking",
            "-DisableStorageSense",
            "-EnableDarkMode",
        };

        public static async Task<int> RunAsync(DebloaterId id, CancellationToken ct = default)
        {
            Logger.Info($"Debloater job '{id}' START");
            int code;
            try
            {
                code = id switch
                {
                    DebloaterId.CttTasks         => await ScriptRunner.RunWinUtilTweaksAsync(DefaultWinUtilTweaks, ct),
                    DebloaterId.RaphiDebloat     => await ScriptRunner.RunWin11DebloatArgsAsync(DefaultWin11DebloatArgs, ct),
                    DebloaterId.AppUninstaller   => await EmbeddedActions.RunAsync("appx_bloat_remove", ct),
                    DebloaterId.EdgeRemover      => await EmbeddedActions.RunAsync("edge_remover_bat", ct),
                    DebloaterId.OrangBoosterTasks => await EmbeddedActions.RunAsync("ob_full_pack", ct),
                    _ => -1,
                };
            }
            catch (System.Exception ex) { Logger.Error($"Debloater job '{id}' threw", ex); return -1; }
            Logger.Info($"Debloater job '{id}' DONE (code={code})");
            return code;
}   }   }