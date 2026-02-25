using System.Net.Http.Json;
using System.Text.Json;
using OmniSharpMCP.Models;

namespace OmniSharpMCP;

/// <summary>
/// HTTP client for communicating with the OmniSharp server.
/// </summary>
public class OmniSharpClient : IDisposable
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;
    private readonly JsonSerializerOptions _jsonOptions;

    public OmniSharpClient(int port = 2050)
    {
        _baseUrl = $"http://localhost:{port}";
        _client = new HttpClient
        {
            BaseAddress = new Uri(_baseUrl),
            Timeout = TimeSpan.FromSeconds(30)
        };

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false
        };
    }

    public async Task<bool> CheckReadyAsync()
    {
        try
        {
            var response = await PostAsync<CheckReadyStatusResponse>("/checkreadystatus", new { });
            return response?.Ready ?? false;
        }
        catch
        {
            return false;
        }
    }

    public async Task<FindUsagesResponse?> FindUsagesAsync(string filePath, int line, int column, bool excludeDefinition = false)
    {
        var request = new FindUsagesRequest
        {
            FileName = filePath,
            Line = line,
            Column = column,
            ExcludeDefinition = excludeDefinition
        };
        return await PostAsync<FindUsagesResponse>("/findusages", request);
    }

    public async Task<GotoDefinitionResponse?> GotoDefinitionAsync(string filePath, int line, int column)
    {
        var request = new GotoDefinitionRequest
        {
            FileName = filePath,
            Line = line,
            Column = column,
            WantMetadata = true
        };
        return await PostAsync<GotoDefinitionResponse>("/v2/gotodefinition", request);
    }

    public async Task<FindImplementationsResponse?> FindImplementationsAsync(string filePath, int line, int column)
    {
        var request = new FindImplementationsRequest
        {
            FileName = filePath,
            Line = line,
            Column = column
        };
        return await PostAsync<FindImplementationsResponse>("/findimplementations", request);
    }

    public async Task<TypeLookupResponse?> GetTypeInfoAsync(string filePath, int line, int column)
    {
        var request = new TypeLookupRequest
        {
            FileName = filePath,
            Line = line,
            Column = column,
            IncludeDocumentation = true
        };
        return await PostAsync<TypeLookupResponse>("/typelookup", request);
    }

    public async Task<CodeCheckResponse?> GetDiagnosticsAsync(string filePath)
    {
        var request = new CodeCheckRequest
        {
            FileName = filePath
        };
        return await PostAsync<CodeCheckResponse>("/codecheck", request);
    }

    public async Task<FindSymbolsResponse?> FindSymbolsAsync(string filter, int? maxItems = null)
    {
        var request = new FindSymbolsRequest
        {
            Filter = filter,
            MaxItemsToReturn = maxItems
        };
        return await PostAsync<FindSymbolsResponse>("/findsymbols", request);
    }

    public async Task<MembersTreeResponse?> GetFileMembersAsync(string filePath)
    {
        var request = new MembersTreeRequest
        {
            FileName = filePath
        };
        return await PostAsync<MembersTreeResponse>("/currentfilemembersastree", request);
    }

    public async Task<List<AutoCompleteItem>?> GetCompletionsAsync(
        string filePath,
        int line,
        int column,
        string wordToComplete,
        string? triggerCharacter = null)
    {
        var request = new AutoCompleteRequest
        {
            FileName = filePath,
            Line = line,
            Column = column,
            WordToComplete = wordToComplete,
            TriggerCharacter = triggerCharacter,
            WantDocumentationForEveryCompletionResult = false,
            WantImportableTypes = true,
            WantMethodHeader = true,
            WantSnippet = true,
            WantReturnType = true,
            WantKind = true
        };
        return await PostAsync<List<AutoCompleteItem>>("/autocomplete", request);
    }

    public async Task<SignatureHelpResponse?> GetSignatureHelpAsync(string filePath, int line, int column)
    {
        var request = new SignatureHelpRequest
        {
            FileName = filePath,
            Line = line,
            Column = column
        };
        return await PostAsync<SignatureHelpResponse>("/signaturehelp", request);
    }

    public async Task<RenameResponse?> RenameAsync(string filePath, int line, int column, string newName, bool applyChanges = false)
    {
        var request = new RenameRequest
        {
            FileName = filePath,
            Line = line,
            Column = column,
            RenameTo = newName,
            WantsTextChanges = true,
            ApplyTextChanges = applyChanges
        };
        return await PostAsync<RenameResponse>("/rename", request);
    }

    public async Task<WorkspaceInfoResponse?> GetWorkspaceInfoAsync()
    {
        return await PostAsync<WorkspaceInfoResponse>("/projects", new { });
    }

    public async Task<MetadataResponse?> GetMetadataAsync(string assemblyName, string typeName, string? projectName = null)
    {
        var request = new MetadataRequest
        {
            AssemblyName = assemblyName,
            TypeName = typeName,
            ProjectName = projectName
        };
        return await PostAsync<MetadataResponse>("/metadata", request);
    }

    private async Task<T?> PostAsync<T>(string endpoint, object request)
    {
        try
        {
            var response = await _client.PostAsJsonAsync(endpoint, request, _jsonOptions);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrEmpty(content))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(content, _jsonOptions);
        }
        catch (HttpRequestException ex)
        {
            throw new OmniSharpException($"Failed to communicate with OmniSharp: {ex.Message}", ex);
        }
        catch (JsonException ex)
        {
            throw new OmniSharpException($"Failed to parse OmniSharp response: {ex.Message}", ex);
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}

public class OmniSharpException : Exception
{
    public OmniSharpException(string message) : base(message) { }
    public OmniSharpException(string message, Exception inner) : base(message, inner) { }
}
