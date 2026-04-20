using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class GetSolutionDiagnosticsTool
{
    [McpServerTool(Name = "get_solution_diagnostics")]
    [Description("Get Roslyn compiler diagnostics (errors, warnings) for the entire loaded solution. Use this after edits to catch cross-file compilation errors, broken references, or type mismatches across the whole project. Filters out Hidden-severity diagnostics.")]
    public static async Task<string> GetSolutionDiagnosticsAsync(OmniSharpClient client)
    {
        var response = await client.GetSolutionDiagnosticsAsync();

        if (response == null || response.QuickFixes.Count == 0)
        {
            return "No diagnostics found. Solution compiles without issues.";
        }

        // Filter out Hidden severity (e.g. unnecessary usings)
        var visible = response.QuickFixes
            .Where(d => !string.Equals(d.LogLevel, "Hidden", StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (visible.Count == 0)
        {
            return "No diagnostics found. Solution compiles without issues.";
        }

        var byFile = visible
            .GroupBy(d => d.FileName)
            .Select(g => new
            {
                file = g.Key,
                diagnostics = g.Select(d => new
                {
                    severity = d.LogLevel,
                    id = d.Id,
                    message = d.Text?.Trim(),
                    line = d.Line,
                    column = d.Column
                })
            });

        var errors = visible.Count(d => string.Equals(d.LogLevel, "Error", StringComparison.OrdinalIgnoreCase));
        var warnings = visible.Count(d => string.Equals(d.LogLevel, "Warning", StringComparison.OrdinalIgnoreCase));

        return JsonSerializer.Serialize(new
        {
            total = visible.Count,
            errors,
            warnings,
            files = byFile
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
