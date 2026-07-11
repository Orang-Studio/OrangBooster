using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OrangBooster
{
    public static class ScriptRunner
    {
        public static readonly string CacheDir = AppPaths.CacheDir;
        public static readonly string AssetsDir = Path.Combine(AppContext.BaseDirectory, "Assets");
        public static readonly string WinUtilCache = Path.Combine(CacheDir, "winutil.ps1");
        public static readonly string EdgeBatPath = Path.Combine(AssetsDir, "edge.bat");
        static readonly string Win11DebloatDir = Path.Combine(CacheDir, "Win11Debloat");
        static readonly string Win11DebloatZip = Path.Combine(CacheDir, "Win11Debloat.zip");
        static string? _win11DebloatScript;
        const string WinUtilUrl = "https://christitus.com/win";
        const string Win11DebloatReleaseApi = "https://api.github.com/repos/Raphire/Win11Debloat/releases/latest";
        const string Win11DebloatMasterZipUrl = "https://github.com/Raphire/Win11Debloat/archive/refs/heads/master.zip";
        const string PsBaseArgs = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -WindowStyle Hidden";
        const string Prelude = "$ProgressPreference='SilentlyContinue';$PSDefaultParameterValues['Start-Process:WindowStyle']='Hidden';";
        static readonly TimeSpan DefaultTimeout = TimeSpan.FromMinutes(5);
        static readonly TimeSpan LongTimeout = TimeSpan.FromMinutes(20);
        static readonly SemaphoreSlim Throttle = new(Math.Max(2, Environment.ProcessorCount / 2));
        static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(120) };
        static readonly HashSet<string> _freshThisSession = new();
        static readonly object _freshGate = new();
        public static volatile string Activity = "";
        static ScriptRunner()
        {
            try { Directory.CreateDirectory(CacheDir); } catch { }
        }
        public static async Task PrefetchAsync()
        {
            Logger.Info("Prefetching winutil + Win11Debloat scripts");
            await Task.WhenAll(
                EnsureScriptAsync(WinUtilUrl, WinUtilCache, "winutil.ps1"),
                EnsureWin11DebloatAsync()
            );
        }
        static string? FindWin11DebloatScript()
        {
            if (!Directory.Exists(Win11DebloatDir)) return null;
            try
            {
                var hits = Directory.GetFiles(Win11DebloatDir, "Win11Debloat.ps1", SearchOption.AllDirectories);
                return hits.Length > 0 ? hits[0] : null;
            }
            catch { return null; }
        }
        static async Task<bool> DownloadWin11DebloatZipAsync(CancellationToken ct)
        {
            Directory.CreateDirectory(CacheDir);
            try
            {
                using var apiReq = new HttpRequestMessage(HttpMethod.Get, Win11DebloatReleaseApi);
                apiReq.Headers.UserAgent.ParseAdd("OrangBooster");
                apiReq.Headers.Accept.ParseAdd("application/vnd.github+json");
                using var apiResp = await Http.SendAsync(apiReq, ct);
                apiResp.EnsureSuccessStatusCode();
                using var doc = JsonDocument.Parse(await apiResp.Content.ReadAsStringAsync(ct));
                string? zipUrl = doc.RootElement.TryGetProperty("zipball_url", out var z) ? z.GetString() : null;
                if (!string.IsNullOrEmpty(zipUrl))
                {
                    using var zipReq = new HttpRequestMessage(HttpMethod.Get, zipUrl);
                    zipReq.Headers.UserAgent.ParseAdd("OrangBooster");
                    using var zipResp = await Http.SendAsync(zipReq, ct);
                    zipResp.EnsureSuccessStatusCode();
                    var bytes = await zipResp.Content.ReadAsByteArrayAsync(ct);
                    if (bytes.Length >= 10_000)
                    {
                        await File.WriteAllBytesAsync(Win11DebloatZip, bytes, ct);
                        Logger.Info($"Win11Debloat latest-release zip downloaded ({bytes.Length} bytes) → {Win11DebloatZip}");
                        return true;
                    }
                    Logger.Warn($"Win11Debloat release zip suspiciously small ({bytes.Length} bytes)");
                }
            }
            catch (Exception ex)
            {
                Logger.Warn($"Win11Debloat latest-release fetch failed: {ex.Message}; falling back to master branch");
            }
            try
            {
                var bytes = await Http.GetByteArrayAsync(Win11DebloatMasterZipUrl, ct);
                if (bytes.Length >= 10_000)
                {
                    await File.WriteAllBytesAsync(Win11DebloatZip, bytes, ct);
                    Logger.Info($"Win11Debloat master zip downloaded ({bytes.Length} bytes) → {Win11DebloatZip}");
                    return true;
                }
                Logger.Warn($"Win11Debloat master zip suspiciously small ({bytes.Length} bytes)");
            }
            catch (Exception ex)
            {
                Logger.Warn($"Win11Debloat master zip download failed: {ex.Message}");
            }
            return false;
        }
        public static async Task<bool> EnsureWin11DebloatAsync(CancellationToken ct = default)
        {
            lock (_freshGate)
            {
                if (_win11DebloatScript != null && File.Exists(_win11DebloatScript))
                    return true;
            }
            var existing = FindWin11DebloatScript();
            if (existing != null)
            {
                Logger.Info($"Win11Debloat already extracted → {existing}");
                lock (_freshGate) _win11DebloatScript = existing;
                return true;
            }
            if (!await DownloadWin11DebloatZipAsync(ct))
                return false;
            try
            {
                if (Directory.Exists(Win11DebloatDir))
                    Directory.Delete(Win11DebloatDir, true);
                Directory.CreateDirectory(Win11DebloatDir);
                ZipFile.ExtractToDirectory(Win11DebloatZip, Win11DebloatDir);
                Logger.Info($"Win11Debloat extracted → {Win11DebloatDir}");
            }
            catch (Exception ex)
            {
                Logger.Error("Win11Debloat zip extraction failed", ex);
                return false;
            }
            var script = FindWin11DebloatScript();
            if (script == null)
            {
                Logger.Error($"Win11Debloat.ps1 not found after extraction under {Win11DebloatDir}");
                return false;
            }
            lock (_freshGate) _win11DebloatScript = script;
            Logger.Info($"Win11Debloat ready → {script}");
            return true;
        }
        public static async Task<bool> EnsureScriptAsync(string url, string cachePath, string bundledName)
        {
            lock (_freshGate)
            {
                if (_freshThisSession.Contains(cachePath) && File.Exists(cachePath))
                    return true;
            }
            try
            {
                Directory.CreateDirectory(CacheDir);
                var s = await Http.GetStringAsync(url);
                if (s.Length > 1000)
                {
                    await File.WriteAllTextAsync(cachePath, s);
                    Logger.Info($"Downloaded {bundledName} ({s.Length} bytes) → {cachePath}");
                    lock (_freshGate) _freshThisSession.Add(cachePath);
                    return true;
                }
                Logger.Warn($"Download of {bundledName} returned suspiciously small payload ({s.Length} bytes), falling back");
            }
            catch (Exception ex) { Logger.Warn($"Download of {bundledName} failed: {ex.Message}"); }
            if (File.Exists(cachePath))
            {
                Logger.Info($"Using cached {bundledName}");
                lock (_freshGate) _freshThisSession.Add(cachePath);
                return true;
            }
            string bundled = Path.Combine(AssetsDir, bundledName);
            if (File.Exists(bundled))
            {
                try
                {
                    File.Copy(bundled, cachePath, true);
                    Logger.Info($"Using bundled fallback {bundledName}");
                    lock (_freshGate) _freshThisSession.Add(cachePath);
                    return true;
                }
                catch (Exception ex) { Logger.Error($"Bundled fallback copy of {bundledName} failed", ex); }
            }
            Logger.Error($"No source available for {bundledName}");
            return false;
        }
        static readonly string ScriptsDir = Path.Combine(CacheDir, "scripts");
        static string WriteScript(string tag, string body)
        {
            Directory.CreateDirectory(ScriptsDir);
            string safe = new string(tag.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
            string path = Path.Combine(ScriptsDir, $"ob-{safe}.ps1");
            File.WriteAllText(path, Prelude + Environment.NewLine + body, new UTF8Encoding(true));
            return path;
        }
        static Task<int> RunScriptBodyAsync(string body, string tag, string? successMarker, string? workingDir, TimeSpan timeout, CancellationToken ct)
        {
            string path = WriteScript(tag, body);
            return RunCoreAsync("powershell.exe", $"{PsBaseArgs} -File \"{path}\"", tag, successMarker, workingDir, timeout, ct);
        }
        public static Task<int> RunPowerShellHiddenAsync(string command, string tag = "ps", CancellationToken ct = default)
            => RunScriptBodyAsync(command, tag, null, null, DefaultTimeout, ct);
        public static Task<int> RunInlinePowerShellAsync(string script, string tag = "inline", CancellationToken ct = default)
            => RunScriptBodyAsync(script, tag, null, null, DefaultTimeout, ct);
        public static Task<int> RunInlinePowerShellAsync(string script, string tag, TimeSpan timeout, CancellationToken ct = default)
            => RunScriptBodyAsync(script, tag, null, null, timeout, ct);
        public static Task<int> RunPowerShellFileAsync(string scriptPath, string args, string tag, string? successMarker, string? workingDir, TimeSpan timeout, CancellationToken ct = default)
        {
            string body = $"& '{scriptPath.Replace("'", "''")}' {args}";
            return RunScriptBodyAsync(body, tag, successMarker, workingDir, timeout, ct);
        }
        public static Task<int> RunBatchHiddenAsync(string batPath, string tag, CancellationToken ct = default)
            => RunCoreAsync("cmd.exe", $"/c \"{batPath}\"", tag, null, null, LongTimeout, ct);
        public static Task<int> RunHiddenAsync(string fileName, string arguments, string tag, CancellationToken ct = default)
            => RunCoreAsync(fileName, arguments, tag, null, null, DefaultTimeout, ct);
        static async Task<int> RunCoreAsync(string fileName, string arguments, string tag, string? successMarker, string? workingDir, TimeSpan timeout, CancellationToken ct)
        {
            await Throttle.WaitAsync(ct);
            string label = $"[{tag}]";
            Logger.Info($"{label} START: {fileName} {Truncate(arguments, 200)}");
            Activity = tag;
            bool markerSeen = false;
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);
            var token = cts.Token;
            try
            {
                var psi = new ProcessStartInfo
                {
                    FileName = fileName,
                    Arguments = arguments,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                };
                if (!string.IsNullOrEmpty(workingDir))
                    psi.WorkingDirectory = workingDir;
                using var proc = Process.Start(psi);
                if (proc == null) { Logger.Error($"{label} failed to start process"); return -1; }
                proc.EnableRaisingEvents = true;
                var exitTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
                proc.Exited += (_, __) => exitTcs.TrySetResult(true);
                proc.OutputDataReceived += (_, e) =>
                {
                    if (e.Data == null) return;
                    Logger.Info($"{label} OUT: {e.Data}");
                    Activity = e.Data;
                    if (successMarker != null && e.Data.Contains(successMarker, StringComparison.OrdinalIgnoreCase))
                        markerSeen = true;
                };
                proc.ErrorDataReceived += (_, e) => { if (e.Data != null) Logger.Warn($"{label} ERR: {e.Data}"); };
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                if (proc.HasExited) exitTcs.TrySetResult(true);
                using (token.Register(() =>
                {
                    try { if (!proc.HasExited) proc.Kill(true); } catch { }
                    exitTcs.TrySetResult(true);
                }))
                {
                    await exitTcs.Task;
                }
                if (token.IsCancellationRequested)
                {
                    if (ct.IsCancellationRequested) { Logger.Warn($"{label} cancelled"); return -1; }
                    Logger.Warn($"{label} TIMEOUT after {timeout.TotalMinutes:0.#} min - process tree killed");
                    return -2;
                }
                int code;
                try { code = proc.ExitCode; } catch { code = markerSeen ? 0 : -1; }
                if (code != 0 && markerSeen)
                {
                    Logger.Info($"{label} exit {code} but success marker seen → treating as OK");
                    code = 0;
                }
                if (code == 0) Logger.Info($"{label} OK");
                else Logger.Warn($"{label} exit {code}");
                return code;
            }
            catch (Exception ex)
            {
                Logger.Error($"{label} threw", ex);
                return -1;
            }
            finally
            {
                Throttle.Release();
            }
        }
        public static async Task<int> RunWinUtilTweaksAsync(IEnumerable<string> tweakNames, CancellationToken ct = default)
        {
            if (!await EnsureScriptAsync(WinUtilUrl, WinUtilCache, "winutil.ps1"))
                return -1;
            var list = tweakNames.Where(t => !string.IsNullOrWhiteSpace(t)).ToArray();
            if (list.Length == 0) return 0;
            string cfg = Path.Combine(CacheDir, $"wu-cfg-{Guid.NewGuid():N}.json");
            await File.WriteAllTextAsync(cfg, JsonSerializer.Serialize(list), ct);
            Logger.Info($"WinUtil config written → {cfg} | tweaks: {string.Join(", ", list)}");
            string args = $"-Config \"{cfg}\" -Noui";
            int code = await RunPowerShellFileAsync(WinUtilCache, args, $"winutil:{list.Length}", "Tweaks are Finished", null, LongTimeout, ct);
            if (code == 1) code = 0;
            try { File.Delete(cfg); } catch { }
            return code;
        }
        public static async Task<int> RunWin11DebloatArgsAsync(IEnumerable<string> args, CancellationToken ct = default)
        {
            if (!await EnsureWin11DebloatAsync(ct) || _win11DebloatScript == null)
                return -1;
            string script = _win11DebloatScript;
            var joined = string.Join(' ', args.Where(a => !string.IsNullOrWhiteSpace(a)));
            if (!joined.Contains("-Silent")) joined = "-Silent " + joined;
            Logger.Info($"Win11Debloat args: {joined}");
            string workingDir = Path.GetDirectoryName(script)!;
            return await RunPowerShellFileAsync(script, joined, "w11d", null, workingDir, LongTimeout, ct);
        }
        public static async Task<int> RunRemoteScriptAsync(string url, string cacheName, string args, string tag, CancellationToken ct = default)
        {
            string cachePath = Path.Combine(CacheDir, cacheName);
            bool ok = false;
            try
            {
                var s = await Http.GetStringAsync(url, ct);
                if (s.Length > 100)
                {
                    await File.WriteAllTextAsync(cachePath, s, ct);
                    Logger.Info($"Downloaded {cacheName} ({s.Length} bytes)");
                    ok = true;
                }
            }
            catch (Exception ex) { Logger.Warn($"Remote fetch of {cacheName} failed: {ex.Message}"); }
            if (!ok && !File.Exists(cachePath))
            {
                string bundled = Path.Combine(AssetsDir, cacheName);
                if (File.Exists(bundled))
                {
                    try { File.Copy(bundled, cachePath, true); ok = true; Logger.Info($"Using bundled {cacheName}"); }
                    catch (Exception ex) { Logger.Error($"Bundled copy of {cacheName} failed", ex); }
                }
            }
            if (!File.Exists(cachePath)) { Logger.Error($"No source for {cacheName}"); return -1; }
            return await RunPowerShellFileAsync(cachePath, args, tag, null, null, DefaultTimeout, ct);
        }
        public static Task<int> RunBundledPowerShellAsync(string bundledName, string args, string tag, CancellationToken ct = default)
        {
            string path = Path.Combine(AssetsDir, bundledName);
            if (!File.Exists(path)) { Logger.Error($"Bundled script missing: {bundledName}"); return Task.FromResult(-1); }
            return RunPowerShellFileAsync(path, args, tag, null, null, DefaultTimeout, ct);
        }
        static string Truncate(string s, int n) => s.Length <= n ? s : s.Substring(0, n) + "…";
}   }