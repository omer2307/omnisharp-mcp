using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class FindReferencesTool
{
    [McpServerTool(Name = "find_references")]
    [Description("Find all references to a symbol at a specific position in a C# file. Returns locations where the symbol is used.")]
    public static async Task<string> FindReferencesAsync(
        OmniSharpClient client,
        [Description("Absolute path to the C# file")] string filePath,
        [Description("Line number (1-based)")] int line,
        [Description("Column number (1-based)")] int column,
        [Description("Exclude the definition from results")] bool excludeDefinition = false)
    {
        var response = await client.FindUsagesAsync(filePath, line, column, excludeDefinition);

        if (response == null || response.QuickFixes.Count == 0)
        {
            return "No references found.";
        }

        var results = response.QuickFixes.Select(qf => new
        {
            file = qf.FileName,
            line = qf.Line,
            column = qf.Column,
            preview = qf.Text?.Trim()
        });

        return JsonSerializer.Serialize(new
        {
            count = response.QuickFixes.Count,
            references = results
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
