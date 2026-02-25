using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;
using OmniSharpMCP.Models;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class GetFileMembersTool
{
    [McpServerTool(Name = "get_file_members")]
    [Description("Get the structure/outline of a C# file, including classes, methods, properties, and fields.")]
    public static async Task<string> GetFileMembersAsync(
        OmniSharpClient client,
        [Description("Absolute path to the C# file")] string filePath)
    {
        var response = await client.GetFileMembersAsync(filePath);

        if (response == null || response.TopLevelTypeDefinitions.Count == 0)
        {
            return "No members found in file.";
        }

        var result = new StringBuilder();
        result.AppendLine($"File: {Path.GetFileName(filePath)}");
        result.AppendLine();

        foreach (var type in response.TopLevelTypeDefinitions)
        {
            PrintNode(result, type, 0);
        }

        return result.ToString();
    }

    private static void PrintNode(StringBuilder sb, TypeNode node, int indent)
    {
        var indentStr = new string(' ', indent * 2);
        var name = GetFeatureValue(node.Features, "name") ?? "Unknown";
        var accessibility = GetFeatureValue(node.Features, "accessibility");

        sb.AppendLine($"{indentStr}{node.Kind}: {(accessibility != null ? accessibility + " " : "")}{name}");

        if (node.ChildNodes != null)
        {
            foreach (var child in node.ChildNodes)
            {
                PrintMemberNode(sb, child, indent + 1);
            }
        }
    }

    private static void PrintMemberNode(StringBuilder sb, MemberNode node, int indent)
    {
        var indentStr = new string(' ', indent * 2);
        var name = GetFeatureValue(node.Features, "name") ?? "Unknown";
        var accessibility = GetFeatureValue(node.Features, "accessibility");
        var returnType = GetFeatureValue(node.Features, "returnType");

        var typeInfo = returnType != null ? $": {returnType}" : "";
        sb.AppendLine($"{indentStr}{node.Kind}: {(accessibility != null ? accessibility + " " : "")}{name}{typeInfo}");

        if (node.ChildNodes != null)
        {
            foreach (var child in node.ChildNodes)
            {
                PrintMemberNode(sb, child, indent + 1);
            }
        }
    }

    private static string? GetFeatureValue(List<SyntaxFeature>? features, string name)
    {
        return features?.FirstOrDefault(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase))?.Data;
    }
}
