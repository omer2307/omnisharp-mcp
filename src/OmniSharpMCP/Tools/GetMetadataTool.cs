using System.ComponentModel;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class GetMetadataTool
{
    [McpServerTool(Name = "get_decompiled_source")]
    [Description("Get decompiled source code for a type from a referenced assembly (metadata). Useful when go_to_definition returns a metadata reference.")]
    public static async Task<string> GetDecompiledSourceAsync(
        OmniSharpClient client,
        [Description("Name of the assembly (e.g., 'System.Collections')")] string assemblyName,
        [Description("Full type name (e.g., 'System.Collections.Generic.List`1')")] string typeName,
        [Description("Optional project name context")] string? projectName = null)
    {
        var response = await client.GetMetadataAsync(assemblyName, typeName, projectName);

        if (response == null || string.IsNullOrEmpty(response.Source))
        {
            return $"Unable to get decompiled source for {typeName} from {assemblyName}.";
        }

        return $"// Source: {response.SourceName}\n// Assembly: {assemblyName}\n// Type: {typeName}\n\n{response.Source}";
    }
}
