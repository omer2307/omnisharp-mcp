using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class FindSymbolsTool
{
    [McpServerTool(Name = "find_symbols")]
    [Description("Search for symbols (classes, methods, properties, etc.) in the workspace by name pattern.")]
    public static async Task<string> FindSymbolsAsync(
        OmniSharpClient client,
        [Description("Search filter/pattern for symbol names")] string filter,
        [Description("Maximum number of results to return")] int maxResults = 50)
    {
        var response = await client.FindSymbolsAsync(filter, maxResults);

        if (response == null || response.QuickFixes.Count == 0)
        {
            return $"No symbols found matching '{filter}'.";
        }

        var symbols = response.QuickFixes.Select(s => new
        {
            name = s.Text?.Trim(),
            kind = s.Kind,
            file = s.FileName,
            line = s.Line,
            column = s.Column,
            container = s.ContainingSymbolName
        });

        return JsonSerializer.Serialize(new
        {
            count = response.QuickFixes.Count,
            symbols = symbols
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
