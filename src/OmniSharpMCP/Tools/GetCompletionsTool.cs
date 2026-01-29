using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class GetCompletionsTool
{
    [McpServerTool(Name = "get_completions")]
    [Description("Get code completion suggestions at a specific position in a C# file.")]
    public static async Task<string> GetCompletionsAsync(
        OmniSharpClient client,
        [Description("Absolute path to the C# file")] string filePath,
        [Description("Line number (1-based)")] int line,
        [Description("Column number (1-based)")] int column,
        [Description("Partial word being typed (can be empty)")] string wordToComplete = "",
        [Description("Trigger character (e.g., '.', '(')")] string? triggerCharacter = null)
    {
        var response = await client.GetCompletionsAsync(filePath, line, column, wordToComplete, triggerCharacter);

        if (response == null || response.Count == 0)
        {
            return "No completions available.";
        }

        var completions = response.Take(50).Select(c => new
        {
            text = c.CompletionText,
            displayText = c.DisplayText,
            kind = c.Kind,
            returnType = c.ReturnType,
            description = c.Description,
            requiredImport = c.RequiredNamespaceImport
        });

        return JsonSerializer.Serialize(new
        {
            count = response.Count,
            completions = completions
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
