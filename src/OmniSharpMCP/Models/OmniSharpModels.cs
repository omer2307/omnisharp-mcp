using System.Text.Json.Serialization;

namespace OmniSharpMCP.Models;

// Base request with file position
public class FilePositionRequest
{
    [JsonPropertyName("FileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("Line")]
    public int Line { get; set; }

    [JsonPropertyName("Column")]
    public int Column { get; set; }
}

// Find usages request/response
public class FindUsagesRequest : FilePositionRequest
{
    [JsonPropertyName("OnlyThisFile")]
    public bool OnlyThisFile { get; set; } = false;

    [JsonPropertyName("ExcludeDefinition")]
    public bool ExcludeDefinition { get; set; } = false;
}

public class FindUsagesResponse
{
    [JsonPropertyName("QuickFixes")]
    public List<QuickFix> QuickFixes { get; set; } = new();
}

// Go to definition request/response
public class GotoDefinitionRequest : FilePositionRequest
{
    [JsonPropertyName("WantMetadata")]
    public bool WantMetadata { get; set; } = true;
}

public class GotoDefinitionResponse
{
    [JsonPropertyName("Definitions")]
    public List<Definition>? Definitions { get; set; }
}

public class Definition
{
    [JsonPropertyName("Location")]
    public Location? Location { get; set; }

    [JsonPropertyName("MetadataSource")]
    public MetadataSource? MetadataSource { get; set; }

    [JsonPropertyName("SourceGeneratedFileInfo")]
    public object? SourceGeneratedFileInfo { get; set; }
}

public class Location
{
    [JsonPropertyName("FileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("Range")]
    public Range? Range { get; set; }
}

public class Range
{
    [JsonPropertyName("Start")]
    public Position? Start { get; set; }

    [JsonPropertyName("End")]
    public Position? End { get; set; }
}

public class Position
{
    [JsonPropertyName("Line")]
    public int Line { get; set; }

    [JsonPropertyName("Column")]
    public int Column { get; set; }
}

public class MetadataSource
{
    [JsonPropertyName("AssemblyName")]
    public string AssemblyName { get; set; } = string.Empty;

    [JsonPropertyName("ProjectName")]
    public string ProjectName { get; set; } = string.Empty;

    [JsonPropertyName("TypeName")]
    public string TypeName { get; set; } = string.Empty;
}

// QuickFix is used by multiple endpoints
public class QuickFix
{
    [JsonPropertyName("FileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("Line")]
    public int Line { get; set; }

    [JsonPropertyName("Column")]
    public int Column { get; set; }

    [JsonPropertyName("EndLine")]
    public int EndLine { get; set; }

    [JsonPropertyName("EndColumn")]
    public int EndColumn { get; set; }

    [JsonPropertyName("Text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("Projects")]
    public List<string>? Projects { get; set; }
}

// Find implementations
public class FindImplementationsRequest : FilePositionRequest
{
}

public class FindImplementationsResponse
{
    [JsonPropertyName("QuickFixes")]
    public List<QuickFix> QuickFixes { get; set; } = new();
}

// Type lookup / hover info
public class TypeLookupRequest : FilePositionRequest
{
    [JsonPropertyName("IncludeDocumentation")]
    public bool IncludeDocumentation { get; set; } = true;
}

public class TypeLookupResponse
{
    [JsonPropertyName("Type")]
    public string? Type { get; set; }

    [JsonPropertyName("Documentation")]
    public string? Documentation { get; set; }

    [JsonPropertyName("StructuredDocumentation")]
    public DocumentationComment? StructuredDocumentation { get; set; }
}

public class DocumentationComment
{
    [JsonPropertyName("SummaryText")]
    public string? SummaryText { get; set; }

    [JsonPropertyName("RemarksText")]
    public string? RemarksText { get; set; }

    [JsonPropertyName("ReturnsText")]
    public string? ReturnsText { get; set; }

    [JsonPropertyName("ParamElements")]
    public List<ParamElement>? ParamElements { get; set; }
}

public class ParamElement
{
    [JsonPropertyName("Name")]
    public string? Name { get; set; }

    [JsonPropertyName("Documentation")]
    public string? Documentation { get; set; }
}

// Code check / diagnostics
public class CodeCheckRequest
{
    [JsonPropertyName("FileName")]
    public string FileName { get; set; } = string.Empty;
}

public class CodeCheckResponse
{
    [JsonPropertyName("QuickFixes")]
    public List<DiagnosticLocation> QuickFixes { get; set; } = new();
}

public class DiagnosticLocation : QuickFix
{
    [JsonPropertyName("LogLevel")]
    public string LogLevel { get; set; } = string.Empty;

    [JsonPropertyName("Id")]
    public string? Id { get; set; }

    [JsonPropertyName("Tags")]
    public List<string>? Tags { get; set; }
}

// Find symbols (workspace search)
public class FindSymbolsRequest
{
    [JsonPropertyName("Filter")]
    public string Filter { get; set; } = string.Empty;

    [JsonPropertyName("MinFilterLength")]
    public int? MinFilterLength { get; set; }

    [JsonPropertyName("MaxItemsToReturn")]
    public int? MaxItemsToReturn { get; set; }
}

public class FindSymbolsResponse
{
    [JsonPropertyName("QuickFixes")]
    public List<SymbolLocation> QuickFixes { get; set; } = new();
}

public class SymbolLocation : QuickFix
{
    [JsonPropertyName("Kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("ContainingSymbolName")]
    public string? ContainingSymbolName { get; set; }
}

// Current file members
public class MembersTreeRequest
{
    [JsonPropertyName("FileName")]
    public string FileName { get; set; } = string.Empty;
}

public class MembersTreeResponse
{
    [JsonPropertyName("TopLevelTypeDefinitions")]
    public List<TypeNode> TopLevelTypeDefinitions { get; set; } = new();
}

public class TypeNode
{
    [JsonPropertyName("Kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("Location")]
    public Location? Location { get; set; }

    [JsonPropertyName("ChildNodes")]
    public List<MemberNode>? ChildNodes { get; set; }

    [JsonPropertyName("Features")]
    public List<SyntaxFeature>? Features { get; set; }
}

public class MemberNode
{
    [JsonPropertyName("Kind")]
    public string Kind { get; set; } = string.Empty;

    [JsonPropertyName("Location")]
    public Location? Location { get; set; }

    [JsonPropertyName("ChildNodes")]
    public List<MemberNode>? ChildNodes { get; set; }

    [JsonPropertyName("Features")]
    public List<SyntaxFeature>? Features { get; set; }
}

public class SyntaxFeature
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Data")]
    public string? Data { get; set; }
}

// Autocomplete
public class AutoCompleteRequest : FilePositionRequest
{
    [JsonPropertyName("WordToComplete")]
    public string WordToComplete { get; set; } = string.Empty;

    [JsonPropertyName("WantDocumentationForEveryCompletionResult")]
    public bool WantDocumentationForEveryCompletionResult { get; set; } = false;

    [JsonPropertyName("WantImportableTypes")]
    public bool WantImportableTypes { get; set; } = true;

    [JsonPropertyName("WantMethodHeader")]
    public bool WantMethodHeader { get; set; } = true;

    [JsonPropertyName("WantSnippet")]
    public bool WantSnippet { get; set; } = true;

    [JsonPropertyName("WantReturnType")]
    public bool WantReturnType { get; set; } = true;

    [JsonPropertyName("WantKind")]
    public bool WantKind { get; set; } = true;

    [JsonPropertyName("TriggerCharacter")]
    public string? TriggerCharacter { get; set; }
}

public class AutoCompleteItem
{
    [JsonPropertyName("CompletionText")]
    public string CompletionText { get; set; } = string.Empty;

    [JsonPropertyName("Description")]
    public string? Description { get; set; }

    [JsonPropertyName("DisplayText")]
    public string DisplayText { get; set; } = string.Empty;

    [JsonPropertyName("RequiredNamespaceImport")]
    public string? RequiredNamespaceImport { get; set; }

    [JsonPropertyName("MethodHeader")]
    public string? MethodHeader { get; set; }

    [JsonPropertyName("ReturnType")]
    public string? ReturnType { get; set; }

    [JsonPropertyName("Snippet")]
    public string? Snippet { get; set; }

    [JsonPropertyName("Kind")]
    public string? Kind { get; set; }

    [JsonPropertyName("IsSuggestionMode")]
    public bool IsSuggestionMode { get; set; }

    [JsonPropertyName("Preselect")]
    public bool Preselect { get; set; }
}

// Signature help
public class SignatureHelpRequest : FilePositionRequest
{
}

public class SignatureHelpResponse
{
    [JsonPropertyName("Signatures")]
    public List<SignatureHelpItem> Signatures { get; set; } = new();

    [JsonPropertyName("ActiveSignature")]
    public int ActiveSignature { get; set; }

    [JsonPropertyName("ActiveParameter")]
    public int ActiveParameter { get; set; }
}

public class SignatureHelpItem
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("Documentation")]
    public string? Documentation { get; set; }

    [JsonPropertyName("Parameters")]
    public List<SignatureHelpParameter> Parameters { get; set; } = new();

    [JsonPropertyName("StructuredDocumentation")]
    public DocumentationComment? StructuredDocumentation { get; set; }
}

public class SignatureHelpParameter
{
    [JsonPropertyName("Name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("Label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("Documentation")]
    public string? Documentation { get; set; }
}

// Rename
public class RenameRequest : FilePositionRequest
{
    [JsonPropertyName("RenameTo")]
    public string RenameTo { get; set; } = string.Empty;

    [JsonPropertyName("WantsTextChanges")]
    public bool WantsTextChanges { get; set; } = true;

    [JsonPropertyName("ApplyTextChanges")]
    public bool ApplyTextChanges { get; set; } = false;
}

public class RenameResponse
{
    [JsonPropertyName("Changes")]
    public List<ModifiedFileResponse> Changes { get; set; } = new();

    [JsonPropertyName("ErrorMessage")]
    public string? ErrorMessage { get; set; }
}

public class ModifiedFileResponse
{
    [JsonPropertyName("FileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("Changes")]
    public List<LinePositionSpanTextChange> Changes { get; set; } = new();
}

public class LinePositionSpanTextChange
{
    [JsonPropertyName("NewText")]
    public string NewText { get; set; } = string.Empty;

    [JsonPropertyName("StartLine")]
    public int StartLine { get; set; }

    [JsonPropertyName("StartColumn")]
    public int StartColumn { get; set; }

    [JsonPropertyName("EndLine")]
    public int EndLine { get; set; }

    [JsonPropertyName("EndColumn")]
    public int EndColumn { get; set; }
}

// Project info
public class WorkspaceInfoResponse
{
    [JsonPropertyName("MsBuild")]
    public MsBuildWorkspaceInfo? MsBuild { get; set; }
}

public class MsBuildWorkspaceInfo
{
    [JsonPropertyName("SolutionPath")]
    public string SolutionPath { get; set; } = string.Empty;

    [JsonPropertyName("Projects")]
    public List<MsBuildProjectInfo> Projects { get; set; } = new();
}

public class MsBuildProjectInfo
{
    [JsonPropertyName("ProjectGuid")]
    public string? ProjectGuid { get; set; }

    [JsonPropertyName("Path")]
    public string Path { get; set; } = string.Empty;

    [JsonPropertyName("AssemblyName")]
    public string AssemblyName { get; set; } = string.Empty;

    [JsonPropertyName("TargetFramework")]
    public string TargetFramework { get; set; } = string.Empty;

    [JsonPropertyName("TargetFrameworks")]
    public List<string>? TargetFrameworks { get; set; }

    [JsonPropertyName("SourceFiles")]
    public List<string>? SourceFiles { get; set; }

    [JsonPropertyName("IsExe")]
    public bool IsExe { get; set; }

    [JsonPropertyName("IsUnityProject")]
    public bool IsUnityProject { get; set; }
}

// Check ready status
public class CheckReadyStatusResponse
{
    [JsonPropertyName("Ready")]
    public bool Ready { get; set; }
}

// Metadata endpoint (for decompiled sources)
public class MetadataRequest
{
    [JsonPropertyName("AssemblyName")]
    public string AssemblyName { get; set; } = string.Empty;

    [JsonPropertyName("TypeName")]
    public string TypeName { get; set; } = string.Empty;

    [JsonPropertyName("ProjectName")]
    public string? ProjectName { get; set; }

    [JsonPropertyName("Timeout")]
    public int? Timeout { get; set; }
}

public class MetadataResponse
{
    [JsonPropertyName("SourceName")]
    public string SourceName { get; set; } = string.Empty;

    [JsonPropertyName("Source")]
    public string Source { get; set; } = string.Empty;
}
