using System.ComponentModel;
using System.Text;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class GetSignatureHelpTool
{
    [McpServerTool(Name = "get_signature_help")]
    [Description("Get method signature help at a position (useful when typing method arguments).")]
    public static async Task<string> GetSignatureHelpAsync(
        OmniSharpClient client,
        [Description("Absolute path to the C# file")] string filePath,
        [Description("Line number (1-based)")] int line,
        [Description("Column number (1-based)")] int column)
    {
        var response = await client.GetSignatureHelpAsync(filePath, line, column);

        if (response == null || response.Signatures.Count == 0)
        {
            return "No signature help available.";
        }

        var result = new StringBuilder();
        result.AppendLine($"Active signature: {response.ActiveSignature + 1} of {response.Signatures.Count}");
        result.AppendLine($"Active parameter: {response.ActiveParameter + 1}");
        result.AppendLine();

        for (int i = 0; i < response.Signatures.Count; i++)
        {
            var sig = response.Signatures[i];
            var marker = i == response.ActiveSignature ? " --> " : "     ";
            result.AppendLine($"{marker}{sig.Label}");

            if (!string.IsNullOrEmpty(sig.Documentation))
            {
                result.AppendLine($"        {sig.Documentation}");
            }

            if (sig.Parameters.Count > 0)
            {
                result.AppendLine("        Parameters:");
                for (int j = 0; j < sig.Parameters.Count; j++)
                {
                    var param = sig.Parameters[j];
                    var paramMarker = j == response.ActiveParameter ? "*" : " ";
                    result.AppendLine($"        {paramMarker} {param.Label}: {param.Documentation}");
                }
            }
            result.AppendLine();
        }

        return result.ToString();
    }
}
