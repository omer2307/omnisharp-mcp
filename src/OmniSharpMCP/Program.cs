using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ModelContextProtocol.Server;
using OmniSharpMCP;
using OmniSharpMCP.Tools;

// Get solution path from environment variable or command line
var solutionPath = Environment.GetEnvironmentVariable("OMNISHARP_SOLUTION");
var port = int.TryParse(Environment.GetEnvironmentVariable("OMNISHARP_PORT"), out var p) ? p : 2050;

if (string.IsNullOrEmpty(solutionPath))
{
    // Try to get from args
    for (int i = 0; i < args.Length; i++)
    {
        if (args[i] == "--solution" || args[i] == "-s")
        {
            if (i + 1 < args.Length)
            {
                solutionPath = args[i + 1];
            }
        }
        else if (args[i] == "--port" || args[i] == "-p")
        {
            if (i + 1 < args.Length && int.TryParse(args[i + 1], out var argPort))
            {
                port = argPort;
            }
        }
    }
}

if (string.IsNullOrEmpty(solutionPath))
{
    Console.Error.WriteLine("Error: No solution path specified.");
    Console.Error.WriteLine("Usage: OmniSharpMCP --solution <path-to-sln>");
    Console.Error.WriteLine("   or: Set OMNISHARP_SOLUTION environment variable");
    Environment.Exit(1);
}

if (!File.Exists(solutionPath))
{
    Console.Error.WriteLine($"Error: Solution file not found: {solutionPath}");
    Environment.Exit(1);
}

Console.Error.WriteLine($"[OmniSharpMCP] Solution: {solutionPath}");
Console.Error.WriteLine($"[OmniSharpMCP] OmniSharp port: {port}");

// Create OmniSharp manager and client
var omnisharpManager = new OmniSharpManager(solutionPath, port);
var omnisharpClient = new OmniSharpClient(port);

// Build and run MCP server (start immediately so Claude Code can connect)
var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSingleton(omnisharpClient);
builder.Services.AddSingleton(omnisharpManager);

builder.Services.AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

var app = builder.Build();

// Cleanup on exit
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStopping.Register(() =>
{
    Console.Error.WriteLine("[OmniSharpMCP] Shutting down...");
    omnisharpManager.Stop();
    omnisharpClient.Dispose();
});

// Start OmniSharp in background after MCP server is ready (if not already running)
lifetime.ApplicationStarted.Register(() =>
{
    _ = Task.Run(async () =>
    {
        try
        {
            // Check if OmniSharp is already running
            if (await omnisharpClient.CheckReadyAsync())
            {
                Console.Error.WriteLine("[OmniSharpMCP] OmniSharp is already running and ready.");
                return;
            }

            Console.Error.WriteLine("[OmniSharpMCP] Starting OmniSharp in background...");
            await omnisharpManager.StartAsync();
            Console.Error.WriteLine("[OmniSharpMCP] OmniSharp is ready.");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[OmniSharpMCP] Failed to start OmniSharp: {ex.Message}");
            Console.Error.WriteLine("[OmniSharpMCP] Tools will not work until OmniSharp is running.");
        }
    });
});

Console.Error.WriteLine("[OmniSharpMCP] MCP server starting...");
await app.RunAsync();
