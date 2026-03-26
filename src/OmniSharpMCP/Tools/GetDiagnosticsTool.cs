using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class GetDiagnosticsTool
{
    [McpServerTool(Name = "get_diagnostics")]
    [Description("Get Roslyn compiler diagnostics (errors, warnings, info) for a C# file. Use this to verify code correctness after edits, check for compilation errors, or identify issues only the compiler can detect (type mismatches, missing references, nullable warnings, etc.).")]
    public static async Task<string> GetDiagnosticsAsync(
        OmniSharpClient client,
        [Description("Absolute path to the C# file")] string filePath)
    {
        var response = await client.GetDiagnosticsAsync(filePath);

        if (response == null || response.QuickFixes.Count == 0)
        {
            return "No diagnostics found. File compiles without issues.";
        }

        var diagnostics = response.QuickFixes.Select(d => new
        {
            severity = d.LogLevel,
            id = d.Id,
            message = d.Text?.Trim(),
            line = d.Line,
            column = d.Column,
            endLine = d.EndLine,
            endColumn = d.EndColumn
        });

        var grouped = diagnostics.GroupBy(d => d.severity).ToDictionary(g => g.Key, g => g.ToList());

        return JsonSerializer.Serialize(new
        {
            total = response.QuickFixes.Count,
            errors = grouped.GetValueOrDefault("Error")?.Count ?? 0,
            warnings = grouped.GetValueOrDefault("Warning")?.Count ?? 0,
            diagnostics = diagnostics
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
