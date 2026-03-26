using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class GetTypeInfoTool
{
    [McpServerTool(Name = "get_type_info")]
    [Description("Get the resolved type and XML documentation for a C# symbol at a specific file position. Returns the fully-qualified type (including resolved generics and inferred types) and documentation comments. Use this when you need to understand what type a variable, parameter, or expression resolves to.")]
    public static async Task<string> GetTypeInfoAsync(
        OmniSharpClient client,
        [Description("Absolute path to the C# file")] string filePath,
        [Description("Line number (1-based)")] int line,
        [Description("Column number (1-based)")] int column)
    {
        var response = await client.GetTypeInfoAsync(filePath, line, column);

        if (response == null || string.IsNullOrEmpty(response.Type))
        {
            return "No type information found.";
        }

        var result = new StringBuilder();
        result.AppendLine($"Type: {response.Type}");

        if (!string.IsNullOrEmpty(response.Documentation))
        {
            result.AppendLine($"\nDocumentation:\n{response.Documentation}");
        }
        else if (response.StructuredDocumentation != null)
        {
            var doc = response.StructuredDocumentation;
            if (!string.IsNullOrEmpty(doc.SummaryText))
            {
                result.AppendLine($"\nSummary: {doc.SummaryText}");
            }
            if (!string.IsNullOrEmpty(doc.RemarksText))
            {
                result.AppendLine($"\nRemarks: {doc.RemarksText}");
            }
            if (!string.IsNullOrEmpty(doc.ReturnsText))
            {
                result.AppendLine($"\nReturns: {doc.ReturnsText}");
            }
            if (doc.ParamElements != null && doc.ParamElements.Count > 0)
            {
                result.AppendLine("\nParameters:");
                foreach (var param in doc.ParamElements)
                {
                    result.AppendLine($"  - {param.Name}: {param.Documentation}");
                }
            }
        }

        return result.ToString();
    }
}
