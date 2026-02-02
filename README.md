# OmniSharp MCP Server

A Model Context Protocol (MCP) server that provides C# language intelligence to Claude Code CLI by wrapping the OmniSharp language server.

## What It Does

This MCP server gives Claude Code full C# IDE capabilities:

- **Find Symbols** - Search for classes, methods, properties by name
- **Go to Definition** - Jump to where a symbol is defined
- **Find References** - Find all usages of a symbol across the codebase
- **Find Implementations** - Find classes implementing an interface
- **Get Type Info** - Get type information and documentation
- **Get Diagnostics** - Get compiler errors and warnings
- **Code Completion** - Get autocomplete suggestions
- **Signature Help** - Get method parameter hints
- **Rename Preview** - Preview what a rename would change
- **Decompiled Source** - View decompiled .NET framework code

## Prerequisites

- [.NET SDK 9.0+](https://dotnet.microsoft.com/download) installed
- [Claude Code CLI](https://claude.ai/code) installed
- A C# solution file (`.sln`)

## Installation

### 1. Clone and Build

```bash
git clone https://github.com/your-repo/omnisharp-mcp.git
cd omnisharp-mcp
dotnet publish src/OmniSharpMCP/OmniSharpMCP.csproj -c Release -o publish
```

### 2. Configure Claude Code

Create and add the MCP server to your Claude Code configuration.

**Which is needed Per-project**

A. Create `.mcp.json` in your project root:

```json
{
  "mcpServers": {
    "csharp": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["/path/to/omnisharp-mcp/publish/OmniSharpMCP.dll"],
      "env": {
        "OMNISHARP_SOLUTION": "/path/to/your/project.sln"
      }
    }
  }
}
```

B. Edit the .claude/mcp.json file args and env values.

Example edited mcp.json file in my project:
```json
{
  "mcpServers": {
    "csharp": {
      "type": "stdio",
      "command": "dotnet",
      "args": ["/Users/omersomekhbeachbum/dev/omnisharp-mcp/publish/OmniSharpMCP.dll"],
      "env": {
        "OMNISHARP_SOLUTION": "/Users/omersomekhbeachbum/dev/rummystars-client/rummystars-client.sln"
      }
    }
  }
}
```

### 3. Enable the MCP Server

Create (if not already created) and edit `~/.claude/settings.local.json`:

```json
{
  "enabledMcpjsonServers": ["csharp"],
  "enableAllProjectMcpServers": true
}
```

### 4. Restart Claude Code

The mcp will be able to start whenever all claude instances are closed.

Start claude again:
```bash
claude
```

Verify the server is connected:

```
/mcp
```

You should see `csharp` in the list of connected servers.

## Enable claude to use omnisharp-mcp tool
This is done out of the box! No additional config here is needed.
Whenever claude instance is running - he has all the tools, and you dont need to specify for his what tools to use. Based on the prompt the claude instance will decide what tools will serve him the best to get to the wanted result.

## First Run

On first run, the MCP server will:

1. Start immediately (so Claude Code can connect)
2. Download OmniSharp HTTP server (~50MB) to `~/.omnisharp-mcp/omnisharp/`
3. Start OmniSharp and load your solution in the background

**Note:** The first startup takes 2-3 minutes while OmniSharp loads your solution. Tools will return errors during this time. Subsequent startups are faster if OmniSharp is already running.

## Warming Up OmniSharp (Optional)

To avoid waiting for OmniSharp to load, you can pre-start it:

```bash
./warmup-omnisharp.sh
```

This script:
- Starts OmniSharp if not already running
- Waits until it's fully loaded
- Keeps it running for instant tool availability

### Auto-Start on macOS Login

Create `~/Library/LaunchAgents/com.omnisharp.mcp.plist`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>Label</key>
    <string>com.omnisharp.mcp</string>
    <key>ProgramArguments</key>
    <array>
        <string>/usr/local/share/dotnet/dotnet</string>
        <string>/Users/YOUR_USERNAME/.omnisharp-mcp/omnisharp/OmniSharp.dll</string>
        <string>-s</string>
        <string>/path/to/your/project.sln</string>
        <string>-p</string>
        <string>2050</string>
    </array>
    <key>EnvironmentVariables</key>
    <dict>
        <key>DOTNET_ROLL_FORWARD</key>
        <string>LatestMajor</string>
    </dict>
    <key>RunAtLoad</key>
    <true/>
    <key>KeepAlive</key>
    <true/>
    <key>StandardOutPath</key>
    <string>/tmp/omnisharp.log</string>
    <key>StandardErrorPath</key>
    <string>/tmp/omnisharp.log</string>
</dict>
</plist>
```

Load it:

```bash
launchctl load ~/Library/LaunchAgents/com.omnisharp.mcp.plist
```

## Available Tools

Once connected, Claude Code has access to these tools:

| Tool | Description |
|------|-------------|
| `find_symbols` | Search for symbols by name pattern |
| `go_to_definition` | Navigate to symbol definition |
| `find_references` | Find all references to a symbol |
| `find_implementations` | Find interface implementations |
| `get_type_info` | Get type and documentation info |
| `get_diagnostics` | Get compiler errors/warnings for a file |
| `get_completions` | Get code completion suggestions |
| `get_signature_help` | Get method signature help |
| `get_file_members` | Get outline of a file |
| `get_workspace_info` | Get solution/project information |
| `preview_rename` | Preview rename changes |
| `get_decompiled_source` | Get decompiled metadata source |

## Architecture

```
Claude Code CLI
      |
      | (stdio - JSON-RPC)
      v
OmniSharp MCP Server (this project)
      |
      | (HTTP - REST API)
      v
OmniSharp HTTP Server (port 2050)
      |
      | (Roslyn)
      v
Your C# Solution
```

The MCP server acts as a bridge:
- Receives tool calls from Claude Code via stdio
- Translates them to HTTP requests to OmniSharp
- Returns formatted responses back to Claude

## Configuration

### Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `OMNISHARP_SOLUTION` | Path to your `.sln` file | Required |
| `OMNISHARP_PORT` | OmniSharp HTTP port | `2050` |
| `OMNISHARP_PATH` | Custom OmniSharp DLL path | Auto-download |

### Command Line Arguments

```bash
dotnet OmniSharpMCP.dll --solution /path/to/solution.sln --port 2050
```

## Troubleshooting

### MCP server fails to connect

1. Check `/mcp` in Claude Code to see server status
2. Verify the path in `mcp.json` is correct
3. Ensure `settings.local.json` has `"csharp"` in `enabledMcpjsonServers`

### Tools return errors

OmniSharp may still be loading. Check status:

```bash
curl -X POST http://localhost:2050/checkreadystatus -d '{}'
```

If it returns `{"Ready":false}`, wait for OmniSharp to finish loading.

### OmniSharp won't start

Check the log:

```bash
tail -f /tmp/omnisharp.log
```

Common issues:
- Solution file not found
- .NET SDK not installed
- Port 2050 already in use

### Symbol search returns empty

OmniSharp needs time to index. Large solutions (like Unity projects) can take 2-3 minutes.

### Reset OmniSharp

```bash
# Kill existing processes
pkill -f "omnisharp/OmniSharp.dll"

# Delete downloaded OmniSharp (will re-download on next start)
rm -rf ~/.omnisharp-mcp/omnisharp
```

## Development

### Building from Source

```bash
cd omnisharp-mcp
dotnet build src/OmniSharpMCP/OmniSharpMCP.csproj
```

### Running Locally

```bash
OMNISHARP_SOLUTION="/path/to/solution.sln" dotnet run --project src/OmniSharpMCP/OmniSharpMCP.csproj
```

### Testing OmniSharp Directly

```bash
# Find symbols
curl -X POST http://localhost:2050/findsymbols \
  -H "Content-Type: application/json" \
  -d '{"Filter": "MyClass"}'

# Check ready status
curl -X POST http://localhost:2050/checkreadystatus -d '{}'
```

## License

MIT
