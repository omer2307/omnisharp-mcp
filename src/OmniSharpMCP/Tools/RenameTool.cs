using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class RenameTool
{
    [McpServerTool(Name = "preview_rename")]
    [Description("Preview what changes would be made when renaming a symbol. Does not apply changes, only shows what would change.")]
    public static async Task<string> PreviewRenameAsync(
        OmniSharpClient client,
        [Description("Absolute path to the C# file")] string filePath,
        [Description("Line number (1-based)")] int line,
        [Description("Column number (1-based)")] int column,
        [Description("New name for the symbol")] string newName)
    {
        var response = await client.RenameAsync(filePath, line, column, newName, applyChanges: false);

        if (response == null)
        {
            return "Unable to rename symbol.";
        }

        if (!string.IsNullOrEmpty(response.ErrorMessage))
        {
            return $"Rename error: {response.ErrorMessage}";
        }

        if (response.Changes.Count == 0)
        {
            return "No changes would be made.";
        }

        var fileChanges = response.Changes.Select(fc => new
        {
            file = fc.FileName,
            changes = fc.Changes.Select(c => new
            {
                startLine = c.StartLine,
                startColumn = c.StartColumn,
                endLine = c.EndLine,
                endColumn = c.EndColumn,
                newText = c.NewText
            })
        });

        return JsonSerializer.Serialize(new
        {
            filesAffected = response.Changes.Count,
            totalChanges = response.Changes.Sum(c => c.Changes.Count),
            changes = fileChanges
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
