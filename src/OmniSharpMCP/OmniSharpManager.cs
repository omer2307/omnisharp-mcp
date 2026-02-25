using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;

namespace OmniSharpMCP;

/// <summary>
/// Manages the OmniSharp server process lifecycle.
/// </summary>
public class OmniSharpManager : IDisposable
{
    private Process? _omnisharpProcess;
    private readonly int _port;
    private readonly string? _solutionPath;
    private bool _isRunning;
    private readonly HttpClient _httpClient;

    private const string OmniSharpVersion = "v1.39.13";
    private static readonly string OmniSharpDir = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".omnisharp-mcp",
        "omnisharp");

    public bool IsRunning => _isRunning;
    public int Port => _port;

    public OmniSharpManager(string solutionPath, int port = 2050)
    {
        _solutionPath = solutionPath;
        _port = port;
        _httpClient = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "OmniSharpMCP");
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        if (_isRunning)
        {
            return;
        }

        var omnisharpPath = await FindOrDownloadOmniSharpAsync(cancellationToken);
        if (omnisharpPath == null)
        {
            throw new OmniSharpException("Could not find or download OmniSharp.");
        }

        Console.Error.WriteLine($"[OmniSharpMCP] Starting OmniSharp from: {omnisharpPath}");
        Console.Error.WriteLine($"[OmniSharpMCP] Solution: {_solutionPath}");
        Console.Error.WriteLine($"[OmniSharpMCP] Port: {_port}");

        var arguments = BuildArguments(omnisharpPath);

        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        // Set environment variables for Unity support and .NET version compatibility
        startInfo.Environment["DOTNET_CLI_UI_LANGUAGE"] = "en";
        startInfo.Environment["DOTNET_ROLL_FORWARD"] = "LatestMajor";

        _omnisharpProcess = new Process { StartInfo = startInfo };

        _omnisharpProcess.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.Error.WriteLine($"[OmniSharp] {e.Data}");
            }
        };

        _omnisharpProcess.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                Console.Error.WriteLine($"[OmniSharp Error] {e.Data}");
            }
        };

        try
        {
            _omnisharpProcess.Start();
            _omnisharpProcess.BeginOutputReadLine();
            _omnisharpProcess.BeginErrorReadLine();
        }
        catch (Exception ex)
        {
            throw new OmniSharpException($"Failed to start OmniSharp: {ex.Message}", ex);
        }

        // Wait for server to be ready
        await WaitForServerReadyAsync(cancellationToken);
        _isRunning = true;
        Console.Error.WriteLine("[OmniSharpMCP] OmniSharp server is ready");
    }

    private string BuildArguments(string omnisharpPath)
    {
        var args = new List<string>();

        // DLL path as first argument to dotnet
        args.Add($"\"{omnisharpPath}\"");

        // Solution path
        if (!string.IsNullOrEmpty(_solutionPath))
        {
            args.Add($"-s \"{_solutionPath}\"");
        }

        // HTTP port
        args.Add($"-p {_port}");

        // Host PID for automatic shutdown
        args.Add($"--hostPID {Environment.ProcessId}");

        // Additional settings for better Unity support
        args.Add("--encoding utf-8");

        return string.Join(" ", args);
    }

    private async Task<string?> FindOrDownloadOmniSharpAsync(CancellationToken cancellationToken)
    {
        // Check environment variable first
        var envPath = Environment.GetEnvironmentVariable("OMNISHARP_PATH");
        if (!string.IsNullOrEmpty(envPath) && File.Exists(envPath))
        {
            return envPath;
        }

        // Check common installation locations
        var possiblePaths = GetPossibleOmniSharpPaths();
        foreach (var path in possiblePaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }

        // Check our downloaded version
        var downloadedPath = GetDownloadedOmniSharpPath();
        if (File.Exists(downloadedPath))
        {
            return downloadedPath;
        }

        // Try to download
        Console.Error.WriteLine("[OmniSharpMCP] OmniSharp not found locally, downloading...");
        return await DownloadOmniSharpAsync(cancellationToken);
    }

    private string GetDownloadedOmniSharpPath()
    {
        // Return the DLL path - we'll run it via dotnet command
        return Path.Combine(OmniSharpDir, "OmniSharp.dll");
    }

    private async Task<string?> DownloadOmniSharpAsync(CancellationToken cancellationToken)
    {
        var downloadUrl = GetOmniSharpDownloadUrl();
        if (downloadUrl == null)
        {
            Console.Error.WriteLine("[OmniSharpMCP] Unsupported platform for OmniSharp download");
            return null;
        }

        try
        {
            Console.Error.WriteLine($"[OmniSharpMCP] Downloading OmniSharp from: {downloadUrl}");

            Directory.CreateDirectory(OmniSharpDir);

            var zipPath = Path.Combine(OmniSharpDir, "omnisharp.zip");
            var response = await _httpClient.GetAsync(downloadUrl, cancellationToken);
            response.EnsureSuccessStatusCode();

            await using (var fs = File.Create(zipPath))
            {
                await response.Content.CopyToAsync(fs, cancellationToken);
            }

            Console.Error.WriteLine("[OmniSharpMCP] Extracting OmniSharp...");
            ZipFile.ExtractToDirectory(zipPath, OmniSharpDir, overwriteFiles: true);
            File.Delete(zipPath);

            var omnisharpPath = GetDownloadedOmniSharpPath();

            // Make executable on Unix
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "chmod",
                    Arguments = $"+x \"{omnisharpPath}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                });
                process?.WaitForExit();
            }

            Console.Error.WriteLine($"[OmniSharpMCP] OmniSharp installed to: {omnisharpPath}");
            return omnisharpPath;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OmniSharpMCP] Failed to download OmniSharp: {ex.Message}");
            return null;
        }
    }

    private string? GetOmniSharpDownloadUrl()
    {
        var baseUrl = $"https://github.com/OmniSharp/omnisharp-roslyn/releases/download/{OmniSharpVersion}";

        // Use HTTP version (omnisharp.http) with net6.0 and roll-forward enabled
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";
            return $"{baseUrl}/omnisharp.http-osx-{arch}-net6.0.zip";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";
            return $"{baseUrl}/omnisharp.http-linux-{arch}-net6.0.zip";
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "x64";
            return $"{baseUrl}/omnisharp.http-win-{arch}-net6.0.zip";
        }

        return null;
    }

    private List<string> GetPossibleOmniSharpPaths()
    {
        var paths = new List<string>();
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            // Homebrew installation
            paths.Add("/usr/local/bin/omnisharp");
            paths.Add("/opt/homebrew/bin/omnisharp");
            paths.Add($"{home}/.dotnet/tools/omnisharp");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            paths.Add("/usr/bin/omnisharp");
            paths.Add("/usr/local/bin/omnisharp");
            paths.Add($"{home}/.dotnet/tools/omnisharp");
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            paths.Add($"{home}\\.dotnet\\tools\\omnisharp.exe");
            paths.Add($"{localAppData}\\Programs\\omnisharp\\OmniSharp.exe");
        }

        return paths;
    }

    private async Task WaitForServerReadyAsync(CancellationToken cancellationToken)
    {
        var maxWaitTime = TimeSpan.FromMinutes(3);
        var pollInterval = TimeSpan.FromMilliseconds(500);
        var elapsed = TimeSpan.Zero;

        while (elapsed < maxWaitTime)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_omnisharpProcess == null || _omnisharpProcess.HasExited)
            {
                throw new OmniSharpException("OmniSharp process terminated unexpectedly");
            }

            try
            {
                using var httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(5) };
                var response = await httpClient.PostAsync(
                    $"http://localhost:{_port}/checkreadystatus",
                    new StringContent("{}"),
                    cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync(cancellationToken);
                    if (content.Contains("\"Ready\":true", StringComparison.OrdinalIgnoreCase) ||
                        content.Contains("\"ready\":true", StringComparison.OrdinalIgnoreCase))
                    {
                        return;
                    }
                }
            }
            catch (HttpRequestException)
            {
                // Server not ready yet
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                // Timeout, retry
            }

            await Task.Delay(pollInterval, cancellationToken);
            elapsed += pollInterval;
        }

        throw new OmniSharpException($"OmniSharp failed to become ready within {maxWaitTime.TotalSeconds} seconds");
    }

    public void Stop()
    {
        if (_omnisharpProcess == null || _omnisharpProcess.HasExited)
        {
            return;
        }

        try
        {
            // Try graceful shutdown first
            _omnisharpProcess.Kill(entireProcessTree: true);
            _omnisharpProcess.WaitForExit(5000);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OmniSharpMCP] Error stopping OmniSharp: {ex.Message}");
        }
        finally
        {
            _isRunning = false;
        }
    }

    public void Dispose()
    {
        Stop();
        _omnisharpProcess?.Dispose();
        _httpClient.Dispose();
    }
}
