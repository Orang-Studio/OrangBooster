using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OrangBooster
{
    public static class CliMenu
    {
        sealed class Item
        {
            public string Label = "";
            public string Desc = "";
            public Func<CancellationToken, Task<int>> Run = _ => Task.FromResult(0);
        }

        static readonly Item[] Items =
        {
            new Item { Label = "Chris Titus winutil tweaks",  Desc = "Community winutil privacy/perf tweak pass",  Run = ct => DebloaterTasks.RunAsync(DebloaterId.CttTasks, ct) },
            new Item { Label = "Win11Debloat (Raphire) pass",  Desc = "Raphire Win11Debloat silent pass",           Run = ct => DebloaterTasks.RunAsync(DebloaterId.RaphiDebloat, ct) },
            new Item { Label = "Remove bloatware apps",        Desc = "Strip preinstalled bloat appx packages",      Run = ct => DebloaterTasks.RunAsync(DebloaterId.AppUninstaller, ct) },
            new Item { Label = "Remove Microsoft Edge",        Desc = "Uninstall Microsoft Edge",                    Run = ct => DebloaterTasks.RunAsync(DebloaterId.EdgeRemover, ct) },
            new Item { Label = "OrangBooster tasks pack",      Desc = "Privacy, services, pins, AI/Copilot, etc.",   Run = ct => DebloaterTasks.RunAsync(DebloaterId.OrangBoosterTasks, ct) },
            new Item { Label = "Clean temp files & caches",    Desc = "Wipe temp folders + restart Explorer",        Run = ct => EmbeddedActions.RunAsync("clean_temp_caches", ct) },
        };

        const string BannerB64 =
            "IOKWiOKWiOKWiOKWiOKWiOKWiOKVlyDilojilojilojilojilojilojilZcgIOKWiOKWiOKWiOKWiOKWiOKVlyDilojilojilojilZcgICDilojilojilZcg4paI4paI4paI4paI4paI4paI4pWXIOKWiOKWiOKWiOKWiOKWiOKWiOKVlwrilojilojilZTilZDilZDilZDilojilojilZfilojilojilZTilZDilZDilojilojilZfilojilojilZTilZDilZDilojilojilZfilojilojilojilojilZcgIOKWiOKWiOKVkeKWiOKWiOKVlOKVkOKVkOKVkOKVkOKVnSDilojilojilZTilZDilZDilZDilZDilZ0K4paI4paI4pWRICAg4paI4paI4pWR4paI4paI4paI4paI4paI4paI4pWU4pWd4paI4paI4paI4paI4paI4paI4paI4pWR4paI4paI4pWU4paI4paI4pWXIOKWiOKWiOKVkeKWiOKWiOKVkSAg4paI4paI4paI4pWX4paI4paI4paI4paI4paI4pWXICAK4paI4paI4pWRICAg4paI4paI4pWR4paI4paI4pWU4pWQ4pWQ4paI4paI4pWX4paI4paI4pWU4pWQ4pWQ4paI4paI4pWR4paI4paI4pWR4pWa4paI4paI4pWX4paI4paI4pWR4paI4paI4pWRICAg4paI4paI4pWR4paI4paI4pWU4pWQ4pWQ4pWdICAK4pWa4paI4paI4paI4paI4paI4paI4pWU4pWd4paI4paI4pWRICDilojilojilZHilojilojilZEgIOKWiOKWiOKVkeKWiOKWiOKVkSDilZrilojilojilojilojilZHilZrilojilojilojilojilojilojilZTilZ3ilojilojilojilojilojilojilojilZcKIOKVmuKVkOKVkOKVkOKVkOKVkOKVnSDilZrilZDilZ0gIOKVmuKVkOKVneKVmuKVkOKVnSAg4pWa4pWQ4pWd4pWa4pWQ4pWdICDilZrilZDilZDilZDilZ0g4pWa4pWQ4pWQ4pWQ4pWQ4pWQ4pWdIOKVmuKVkOKVkOKVkOKVkOKVkOKVkOKVnQ==";

        static string[] BannerLines() =>
            Encoding.UTF8.GetString(Convert.FromBase64String(BannerB64))
                    .Replace("\r", "").Split('\n');

        const string Esc = "\x1b";
        const string Orange = Esc + "[38;2;255;140;0m";
        const string Dim = Esc + "[90m";
        const string Green = Esc + "[32m";
        const string Red = Esc + "[31m";
        const string Reset = Esc + "[0m";
        const string SelBg = Esc + "[48;2;255;140;0m" + Esc + "[30m";
        const string Pad = "   ";

        const int InnerWidth = 52;
        static readonly string BoxTL = char.ConvertFromUtf32(0x2554);
        static readonly string BoxTR = char.ConvertFromUtf32(0x2557);
        static readonly string BoxBL = char.ConvertFromUtf32(0x255A);
        static readonly string BoxBR = char.ConvertFromUtf32(0x255D);
        static readonly string BoxH  = char.ConvertFromUtf32(0x2550);
        static readonly string BoxV  = char.ConvertFromUtf32(0x2551);
        static readonly string BoxML = char.ConvertFromUtf32(0x2560);
        static readonly string BoxMR = char.ConvertFromUtf32(0x2563);
        static readonly string Check = char.ConvertFromUtf32(0x2714);
        static readonly string Cross = char.ConvertFromUtf32(0x2718);
        static readonly string[] Spin =
        {
            char.ConvertFromUtf32(0x280B), char.ConvertFromUtf32(0x2819), char.ConvertFromUtf32(0x2839),
            char.ConvertFromUtf32(0x2838), char.ConvertFromUtf32(0x283C), char.ConvertFromUtf32(0x2834),
            char.ConvertFromUtf32(0x2826), char.ConvertFromUtf32(0x2827), char.ConvertFromUtf32(0x2807),
            char.ConvertFromUtf32(0x280F),
        };

        public static async Task<int> RunAsync()
        {
            SetupConsole();

            List<Item> chosen = SelectWithTui();

            int worst = 0;
            if (chosen.Count == 0)
            {
                WriteLine($"{Dim}Nothing selected.{Reset}");
            }
            else
            {
                var results = await RunTasks(chosen);
                worst = results.Any(r => r.code != 0) ? 1 : 0;
            }

            Goodbye();
            return worst;
        }

        static List<Item> SelectWithTui()
        {
            var rows = Items.ToList();
            var full = new Item { Label = "Full debloat (everything)", Desc = "Run every task above" };
            rows.Add(full);

            if (_hIn == InvalidHandle)
                return NumberedSelect();

            var ticked = new bool[rows.Count];
            int cursor = 0;
            try { Console.CursorVisible = false; } catch { }
            try
            {
                while (true)
                {
                    DrawTui(rows, ticked, cursor);
                    int vk = ReadVk();
                    switch (vk)
                    {
                        case VK_UP:    cursor = (cursor - 1 + rows.Count) % rows.Count; break;
                        case VK_DOWN:  cursor = (cursor + 1) % rows.Count; break;
                        case VK_SPACE: ticked[cursor] = !ticked[cursor]; break;
                        case VK_A:
                            bool all = ticked.Take(Items.Length).All(t => t);
                            for (int i = 0; i < Items.Length; i++) ticked[i] = !all;
                            break;
                        case VK_RETURN:
                            if (ticked[rows.Count - 1]) return Items.ToList();
                            return Enumerable.Range(0, Items.Length).Where(i => ticked[i]).Select(i => Items[i]).ToList();
                        case VK_Q:
                        case VK_ESCAPE:
                        case -1:
                            return new List<Item>();
                    }
                }
            }
            finally { try { Console.CursorVisible = true; } catch { } }
        }

        static void DrawTui(List<Item> rows, bool[] ticked, int cursor)
        {
            Console.Clear();
            WriteLine("");
            foreach (var line in BannerLines()) WriteLine($"{Pad}{Orange}{line}{Reset}");
            WriteLine($"{Pad}{Dim}   Windows debloater by OrangStudio{Reset}");
            WriteLine("");
            WriteLine(Rule(BoxTL, BoxTR));
            BLine(Center("Select what to debloat", InnerWidth), Orange);
            WriteLine(Rule(BoxML, BoxMR));
            for (int i = 0; i < rows.Count; i++)
            {
                bool isFull = i == rows.Count - 1;
                if (isFull) WriteLine(Rule(BoxML, BoxMR));
                string box = ticked[i] ? "[x]" : "[ ]";
                string arrow = i == cursor ? "> " : "  ";
                string content = $" {arrow}{box} {rows[i].Label}";
                if (i == cursor) BSelLine(content);
                else BLine(content, isFull ? Orange : "");
            }
            WriteLine(Rule(BoxML, BoxMR));
            BLine(Center("Up/Dn move  Spc tick  A all  Enter run  Q quit", InnerWidth), Dim);
            WriteLine(Rule(BoxBL, BoxBR));
            WriteLine($"{Pad}{Dim}{rows[cursor].Desc}{Reset}");
        }

        static string Rule(string left, string right)
        {
            var bar = new System.Text.StringBuilder();
            for (int i = 0; i < InnerWidth; i++) bar.Append(BoxH);
            return $"{Pad}{Orange}{left}{bar}{right}{Reset}";
        }

        static string Fit(string s)
        {
            if (s.Length > InnerWidth) return s.Substring(0, InnerWidth);
            return s.PadRight(InnerWidth);
        }

        static void BLine(string visible, string color)
        {
            WriteLine($"{Pad}{Orange}{BoxV}{Reset}{color}{Fit(visible)}{Reset}{Orange}{BoxV}{Reset}");
        }

        static void BSelLine(string visible)
        {
            WriteLine($"{Pad}{Orange}{BoxV}{Reset}{SelBg}{Fit(visible)}{Reset}{Orange}{BoxV}{Reset}");
        }

        static string Center(string s, int width)
        {
            if (s.Length >= width) return s.Substring(0, width);
            int pad = width - s.Length;
            int left = pad / 2;
            return new string(' ', left) + s + new string(' ', pad - left);
        }

        static List<Item> NumberedSelect()
        {
            WriteLine("");
            foreach (var line in BannerLines()) WriteLine($"{Pad}{Orange}{line}{Reset}");
            WriteLine("");
            for (int i = 0; i < Items.Length; i++)
                WriteLine($"{Pad}{i + 1}) {Items[i].Label}  -  {Items[i].Desc}");
            WriteLine($"{Pad}{Items.Length + 1}) Full debloat (everything)");
            WriteLine($"{Pad}0) Quit");
            Console.Write("Enter numbers (space/comma separated), or 0 to quit: ");
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) return new List<Item>();
            var tokens = input.Split(new[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            var pick = new List<Item>();
            foreach (var t in tokens)
            {
                if (!int.TryParse(t, out int n)) continue;
                if (n == 0) return new List<Item>();
                if (n == Items.Length + 1) return Items.ToList();
                if (n >= 1 && n <= Items.Length && !pick.Contains(Items[n - 1])) pick.Add(Items[n - 1]);
            }
            return pick;
        }

        static async Task<List<(Item item, int code)>> RunTasks(List<Item> chosen)
        {
            Console.Clear();
            WriteLine("");
            foreach (var line in BannerLines()) WriteLine($"{Pad}{Orange}{line}{Reset}");
            WriteLine("");
            WriteLine($"{Pad}{Orange}Running {chosen.Count} task(s)  {Dim}(Ctrl+C to abort){Reset}");
            WriteLine("");
            var results = new List<(Item, int)>();
            try { Console.CursorVisible = false; } catch { }
            for (int i = 0; i < chosen.Count; i++)
            {
                var it = chosen[i];
                string tagPlain = $"[{i + 1}/{chosen.Count}]";
                ScriptRunner.Activity = "";
                var work = Task.Run(() => it.Run(CancellationToken.None));
                var sw = Stopwatch.StartNew();
                int f = 0;
                while (!work.IsCompleted)
                {
                    string secStr = $"({sw.Elapsed.TotalSeconds:0.0}s)";
                    int used = Pad.Length + tagPlain.Length + it.Label.Length + secStr.Length + 8;
                    string act = Clip(ScriptRunner.Activity, ConsoleWidth() - used);
                    string extra = act.Length > 0 ? $" {Dim}{act}{Reset}" : "";
                    CWrite($"{Pad}{Orange}{Spin[f % Spin.Length]}{Reset} {Dim}{tagPlain}{Reset} {it.Label}{extra} {Dim}{secStr}{Reset}");
                    f++;
                    try { await Task.Delay(90); } catch { }
                }
                int code;
                try { code = await work; }
                catch (Exception ex) { Logger.Error($"CLI item '{it.Label}' threw", ex); code = -1; }
                results.Add((it, code));
                string mark = code == 0 ? $"{Green}{Check}{Reset}" : $"{Red}{Cross}{Reset}";
                string res = code == 0 ? $"{Green}ok{Reset}" : $"{Red}fail {code}{Reset}";
                CWrite($"{Pad}{mark} {Dim}{tagPlain}{Reset} {it.Label} {Dim}({sw.Elapsed.TotalSeconds:0.0}s){Reset}  {res}");
                WriteLine("");
            }
            try { Console.CursorVisible = true; } catch { }
            WriteLine("");
            int okCount = results.Count(r => r.Item2 == 0);
            string summary = okCount == results.Count
                ? $"{Pad}{Green}All {results.Count} task(s) succeeded.{Reset}"
                : $"{Pad}{Orange}Done.{Reset} {Green}{okCount} ok{Reset}, {Red}{results.Count - okCount} failed{Reset}.";
            WriteLine(summary);
            return results;
        }

        static void CWrite(string content)
        {
            int w = 100;
            try { w = Math.Max(1, Console.WindowWidth - 1); } catch { }
            try { Console.Write("\r" + new string(' ', w) + "\r" + content); } catch { }
        }

        static int ConsoleWidth()
        {
            try { return Math.Max(40, Console.WindowWidth); } catch { return 80; }
        }

        static void SizeWindow()
        {
            try
            {
                int w = Math.Min(84, Console.LargestWindowWidth);
                int h = Math.Min(32, Console.LargestWindowHeight);
                Console.SetWindowSize(Math.Min(w, Console.WindowWidth), Math.Min(h, Console.WindowHeight));
                Console.SetBufferSize(w, Math.Max(h, 1000));
                Console.SetWindowSize(w, h);
            }
            catch { }
        }

        static string Clip(string s, int max)
        {
            if (string.IsNullOrEmpty(s) || max <= 1) return "";
            s = s.Replace('\r', ' ').Replace('\n', ' ').Trim();
            if (s.Length <= max) return s;
            return s.Substring(0, Math.Max(0, max - 3)) + "...";
        }

        static void Goodbye()
        {
            WriteLine("");
            string msg = "thanks for using our products, bye :>";
            var bar = new System.Text.StringBuilder();
            for (int i = 0; i < msg.Length + 4; i++) bar.Append(BoxH);
            WriteLine($"{Pad}{Orange}{BoxTL}{bar}{BoxTR}{Reset}");
            WriteLine($"{Pad}{Orange}{BoxV}{Reset}  {Orange}{msg}{Reset}  {Orange}{BoxV}{Reset}");
            WriteLine($"{Pad}{Orange}{BoxBL}{bar}{BoxBR}{Reset}");
            WriteLine($"{Pad}{Dim}Press any key to close...{Reset}");
            if (_hIn != InvalidHandle) ReadVk();
            else { try { Console.ReadKey(true); } catch { } }
        }

        static void WriteLine(string s) { try { Console.WriteLine(s); } catch { } }

        const int STD_OUTPUT_HANDLE = -11;
        const uint ENABLE_VIRTUAL_TERMINAL_PROCESSING = 0x0004;
        const uint GENERIC_READ = 0x80000000;
        const uint GENERIC_WRITE = 0x40000000;
        const uint FILE_SHARE_READ = 0x1;
        const uint FILE_SHARE_WRITE = 0x2;
        const uint OPEN_EXISTING = 3;
        const uint ENABLE_PROCESSED_INPUT = 0x0001;
        const uint ENABLE_LINE_INPUT = 0x0002;
        const uint ENABLE_ECHO_INPUT = 0x0004;
        const uint ENABLE_EXTENDED_FLAGS = 0x0080;
        const uint ENABLE_QUICK_EDIT_INPUT = 0x0040;
        const int KEY_EVENT = 1;
        const int VK_RETURN = 0x0D;
        const int VK_ESCAPE = 0x1B;
        const int VK_SPACE = 0x20;
        const int VK_UP = 0x26;
        const int VK_DOWN = 0x28;
        const int VK_A = 0x41;
        const int VK_Q = 0x51;
        static readonly IntPtr InvalidHandle = new IntPtr(-1);
        static IntPtr _hIn = new IntPtr(-1);

        [DllImport("kernel32.dll")] static extern IntPtr GetStdHandle(int nStdHandle);
        [DllImport("kernel32.dll")] static extern bool GetConsoleMode(IntPtr h, out uint mode);
        [DllImport("kernel32.dll")] static extern bool SetConsoleMode(IntPtr h, uint mode);
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)] static extern bool FreeConsole();
        [DllImport("kernel32.dll")] static extern bool SetConsoleCtrlHandler(HandlerRoutine? handler, bool add);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr CreateFileW(string name, uint access, uint share, IntPtr sec, uint disp, uint flags, IntPtr template);
        [DllImport("kernel32.dll")]
        static extern bool ReadConsoleInputW(IntPtr h, [Out] INPUT_RECORD[] buffer, uint length, out uint read);

        delegate bool HandlerRoutine(uint ctrlType);
        static HandlerRoutine? _ctrlHandler;
        static bool OnCtrl(uint ctrlType)
        {
            try
            {
                WriteLine("");
                WriteLine($"{Orange}Aborted - thanks for using our products, bye :>{Reset}");
            }
            catch { }
            Environment.Exit(0);
            return true;
        }

        [StructLayout(LayoutKind.Explicit)]
        struct INPUT_RECORD
        {
            [FieldOffset(0)] public ushort EventType;
            [FieldOffset(4)] public KEY_EVENT_RECORD KeyEvent;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KEY_EVENT_RECORD
        {
            public int bKeyDown;
            public ushort wRepeatCount;
            public ushort wVirtualKeyCode;
            public ushort wVirtualScanCode;
            public char UnicodeChar;
            public uint dwControlKeyState;
        }

        static void SetupConsole()
        {
            try { FreeConsole(); } catch { }
            try { AllocConsole(); } catch { }

            try
            {
                var sw = new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true };
                Console.SetOut(sw);
                var se = new StreamWriter(Console.OpenStandardError()) { AutoFlush = true };
                Console.SetError(se);
            }
            catch { }

            try
            {
                var hOut = GetStdHandle(STD_OUTPUT_HANDLE);
                if (GetConsoleMode(hOut, out uint om))
                    SetConsoleMode(hOut, om | ENABLE_VIRTUAL_TERMINAL_PROCESSING);
            }
            catch { }

            try { Console.OutputEncoding = Encoding.UTF8; } catch { }
            try { Console.Title = "OrangBooster CLI"; } catch { }
            SizeWindow();

            try
            {
                _hIn = CreateFileW("CONIN$", GENERIC_READ | GENERIC_WRITE,
                    FILE_SHARE_READ | FILE_SHARE_WRITE, IntPtr.Zero, OPEN_EXISTING, 0, IntPtr.Zero);
                if (_hIn != InvalidHandle && GetConsoleMode(_hIn, out uint im))
                {
                    im &= ~(ENABLE_LINE_INPUT | ENABLE_ECHO_INPUT | ENABLE_QUICK_EDIT_INPUT);
                    im |= ENABLE_EXTENDED_FLAGS | ENABLE_PROCESSED_INPUT;
                    SetConsoleMode(_hIn, im);
                }
            }
            catch { _hIn = InvalidHandle; }

            try { _ctrlHandler = OnCtrl; SetConsoleCtrlHandler(_ctrlHandler, true); } catch { }
        }

        static int ReadVk()
        {
            if (_hIn == InvalidHandle) return -1;
            var rec = new INPUT_RECORD[1];
            while (true)
            {
                if (!ReadConsoleInputW(_hIn, rec, 1, out uint read) || read == 0) return -1;
                if (rec[0].EventType == KEY_EVENT && rec[0].KeyEvent.bKeyDown != 0)
                    return rec[0].KeyEvent.wVirtualKeyCode;
            }
        }
    }
}
