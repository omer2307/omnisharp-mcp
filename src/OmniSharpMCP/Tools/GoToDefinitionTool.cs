using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class GoToDefinitionTool
{
    [McpServerTool(Name = "go_to_definition")]
    [Description("Resolve the definition location of a C# symbol at a given file position. Returns the source file and line where the symbol is declared, or a metadata reference for framework/NuGet types. Use this to trace where a type, method, or variable is defined — especially when the definition is in another assembly or generated code that text search cannot find.")]
    public static async Task<string> GoToDefinitionAsync(
        OmniSharpClient client,
        [Description("Absolute path to the C# file")] string filePath,
        [Description("Line number (1-based)")] int line,
        [Description("Column number (1-based)")] int column)
    {
        var response = await client.GotoDefinitionAsync(filePath, line, column);

        if (response?.Definitions == null || response.Definitions.Count == 0)
        {
            return "Definition not found.";
        }

        var definitions = response.Definitions.Select(def =>
        {
            if (def.Location != null)
            {
                return new
                {
                    file = def.Location.FileName,
                    line = def.Location.Range?.Start?.Line ?? 0,
                    column = def.Location.Range?.Start?.Column ?? 0,
                    isMetadata = false as bool?
                };
            }
            else if (def.MetadataSource != null)
            {
                return new
                {
                    file = $"[{def.MetadataSource.AssemblyName}] {def.MetadataSource.TypeName}",
                    line = 0,
                    column = 0,
                    isMetadata = true as bool?
                };
            }
            return null;
        }).Where(d => d != null);

        return JsonSerializer.Serialize(new
        {
            count = response.Definitions.Count,
            definitions = definitions
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
