using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

namespace OmniSharpMCP.Tools;

[McpServerToolType]
public static class GetWorkspaceInfoTool
{
    [McpServerTool(Name = "get_workspace_info")]
    [Description("Get information about the loaded solution/projects including project paths, target frameworks, and references.")]
    public static async Task<string> GetWorkspaceInfoAsync(OmniSharpClient client)
    {
        var response = await client.GetWorkspaceInfoAsync();

        if (response?.MsBuild == null)
        {
            return "No workspace information available.";
        }

        var projects = response.MsBuild.Projects.Select(p => new
        {
            name = p.AssemblyName,
            path = p.Path,
            targetFramework = p.TargetFramework,
            targetFrameworks = p.TargetFrameworks,
            isExe = p.IsExe,
            isUnityProject = p.IsUnityProject,
            sourceFileCount = p.SourceFiles?.Count ?? 0
        });

        return JsonSerializer.Serialize(new
        {
            solutionPath = response.MsBuild.SolutionPath,
            projectCount = response.MsBuild.Projects.Count,
            projects = projects
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
