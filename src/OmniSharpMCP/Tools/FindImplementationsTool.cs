using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class FindImplementationsTool
{
    [McpServerTool(Name = "find_implementations")]
    [Description("Find all concrete implementations of a C# interface, abstract class, or virtual/override member across the solution. Use this to discover which classes implement an interface or override a method — semantic analysis that text search cannot reliably perform.")]
    public static async Task<string> FindImplementationsAsync(
        OmniSharpClient client,
        [Description("Absolute path to the C# file")] string filePath,
        [Description("Line number (1-based)")] int line,
        [Description("Column number (1-based)")] int column)
    {
        var response = await client.FindImplementationsAsync(filePath, line, column);

        if (response == null || response.QuickFixes.Count == 0)
        {
            return "No implementations found.";
        }

        var implementations = response.QuickFixes.Select(qf => new
        {
            file = qf.FileName,
            line = qf.Line,
            column = qf.Column,
            preview = qf.Text?.Trim()
        });

        return JsonSerializer.Serialize(new
        {
            count = response.QuickFixes.Count,
            implementations = implementations
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
