using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
namespace OrangBooster
{
    public static class CliRunner
    {
        [DllImport("kernel32.dll")] static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll")] static extern bool AllocConsole();
        const int ATTACH_PARENT_PROCESS = -1;
        static readonly (string key, DebloaterId id, string desc)[] Categories =
        {
            ("ctt",   DebloaterId.CttTasks,         "Chris Titus winutil tweaks"),
            ("raphi", DebloaterId.RaphiDebloat,     "Win11Debloat (Raphire) pass"),
            ("apps",  DebloaterId.AppUninstaller,   "Remove bloatware appx packages"),
            ("edge",  DebloaterId.EdgeRemover,      "Remove Microsoft Edge"),
            ("ob",    DebloaterId.OrangBoosterTasks,"OrangBooster tasks pack (privacy, services, pins, etc.)"),
        };
        public static bool IsCliInvocation(string[] args) =>
            args.Any(a => a is "--debloat" or "--boost" or "--list" or "--help" or "-h" or "--cli");
        public static bool WantsElevation(string[] args) =>
            args.Any(a => a is "--debloat" or "--boost" or "--cli");

        static bool IsInteractive(string[] args) =>
            args.Any(a => a == "--cli") &&
            !args.Any(a => a is "--debloat" or "--boost" or "--list" or "--help" or "-h");
        static void Out(string s) { try { Console.WriteLine(s); } catch { } }

        static List<string> ValuesAfter(string[] args, int flag)
        {
            var vals = new List<string>();
            for (int i = flag + 1; i < args.Length && !args[i].StartsWith("-"); i++)
                vals.Add(args[i].ToLowerInvariant());
            return vals;
        }

        public static async Task<int> RunAsync(string[] args)
        {
            if (!AttachConsole(ATTACH_PARENT_PROCESS)) AllocConsole();

            if (IsInteractive(args)) return await CliMenu.RunAsync();

            if (args.Any(a => a is "--help" or "-h")) { PrintHelp(); return 0; }
            if (args.Any(a => a == "--list")) { PrintList(); return 0; }

            int worst = 0;
            bool didSomething = false;

            int di = Array.IndexOf(args, "--debloat");
            if (di >= 0)
            {
                didSomething = true;
                var cats = ValuesAfter(args, di);
                if (cats.Count == 0) cats.Add("full");
                int r = await RunDebloatAsync(cats);
                if (r != 0) worst = r;
            }

            int bi = Array.IndexOf(args, "--boost");
            if (bi >= 0)
            {
                didSomething = true;
                var keys = ValuesAfter(args, bi);
                if (keys.Count == 0)
                {
                    Out("--boost needs a card key, 'all', or 'recommended'. Use --list to see keys.");
                    return 1;
                }
                int r = await RunBoostAsync(keys);
                if (r != 0) worst = r;
            }

            if (!didSomething) { PrintHelp(); return 1; }
            Out($"OrangBooster CLI finished (code {worst})");
            return worst;
        }

        static async Task<int> RunDebloatAsync(List<string> cats)
        {
            int worst = 0;
            if (cats.Contains("full"))
            {
                Out("OrangBooster CLI = running FULL debloat");
                foreach (var c in Categories)
                {
                    Out($"-> {c.key}: {c.desc}");
                    int r = await DebloaterTasks.RunAsync(c.id);
                    if (r != 0) worst = r;
                    Out($"   done (code {r})");
                }
                Out("-> clean temp/caches + restart explorer");
                int cr = await EmbeddedActions.RunAsync("clean_temp_caches");
                if (cr != 0) worst = cr;
                return worst;
            }
            foreach (var cat in cats)
            {
                var match = Categories.FirstOrDefault(c => c.key == cat);
                if (match.key == null) { Out($"Unknown debloat category '{cat}'. Use --list."); worst = 1; continue; }
                Out($"-> debloat {match.key}: {match.desc}");
                int r = await DebloaterTasks.RunAsync(match.id);
                if (r != 0) worst = r;
                Out($"   done (code {r})");
            }
            return worst;
        }

        static async Task<int> RunBoostAsync(List<string> keys)
        {
            var all = BoosterCatalog.Build().ToList();
            List<BoosterCard> toRun;
            if (keys.Contains("all")) toRun = all;
            else if (keys.Contains("recommended")) toRun = all.Where(c => c.Recommended).ToList();
            else
            {
                toRun = new List<BoosterCard>();
                foreach (var k in keys)
                {
                    var card = all.FirstOrDefault(c => c.Key == k);
                    if (card == null) { Out($"Unknown booster key '{k}'. Use --list."); continue; }
                    toRun.Add(card);
                }
            }
            if (toRun.Count == 0) { Out("No matching booster cards selected."); return 1; }
            Out($"OrangBooster CLI = running {toRun.Count} booster card(s)");
            int worst = 0;
            foreach (var card in toRun)
            {
                Out($"-> {card.Key}: {card.Title}");
                int r = await RunCardAsync(card);
                if (r != 0) worst = r;
                Out($"   done (code {r})");
            }
            return worst;
        }

        static async Task<int> RunCardAsync(BoosterCard card)
        {
            int worst = 0;
            try
            {
                if (card.WinUtilTweaks.Length > 0)
                {
                    int r = await ScriptRunner.RunWinUtilTweaksAsync(card.WinUtilTweaks);
                    if (r != 0) worst = r;
                }
                if (card.Win11DebloatArgs.Length > 0)
                {
                    int r = await ScriptRunner.RunWin11DebloatArgsAsync(card.Win11DebloatArgs);
                    if (r != 0) worst = r;
                }
                foreach (var act in card.EmbeddedActions)
                {
                    int r = await EmbeddedActions.RunAsync(act);
                    if (r != 0) worst = r;
                }
            }
            catch (Exception ex) { Logger.Error($"CLI card '{card.Title}' threw", ex); return -1; }
            return worst;
        }

        static void PrintList()
        {
            Out("Debloat categories  (--debloat <cat> [<cat> ...]):");
            Out("  full              run every category below, then clean temp/caches");
            foreach (var c in Categories) Out($"  {c.key,-18}{c.desc}");
            Out("");
            var all = BoosterCatalog.Build().ToList();
            Out($"Booster cards  (--boost <key> [<key> ...] | all | recommended) = {all.Count} cards (* = recommended):");
            foreach (var c in all)
                Out($"  {c.Key,-44} {c.Title}{(c.Recommended ? " *" : "")}");
        }

        static void PrintHelp()
        {
            Out("OrangBooster - command-line interface");
            Out("");
            Out("Usage:");
            Out("  OrangBooster.exe --cli                 interactive debloater menu (arrow-key select)");
            Out("  OrangBooster.exe --debloat [cat ...]   run debloat categories (default: full)");
            Out("  OrangBooster.exe --boost <key ...>     run specific booster cards by key");
            Out("  OrangBooster.exe --boost all           run every booster card");
            Out("  OrangBooster.exe --boost recommended   run the recommended booster cards");
            Out("  OrangBooster.exe --list                list debloat categories + all booster keys");
            Out("  OrangBooster.exe --help                show this help");
            Out("");
            Out("Debloat and boost can be combined, e.g.:");
            Out("  OrangBooster.exe --debloat ob --boost remove-windows-terminal ultimate-performance-power-plan");
            Out("");
            Out("Note: --debloat/--boost require admin; the process re-launches elevated automatically.");
        }
    }
}
