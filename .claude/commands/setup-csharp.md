---
allowed-tools: Bash, Glob, Read
description: Find dotnet and .sln file, then configure the C# MCP server
---

Set up the omnisharp-mcp server for this project. Do the following steps in order:

## 1. Find the .sln file

Use Glob to search for `**/*.sln` in the current working directory. If multiple are found, ask the user which one to use. If none found, tell the user this project has no .sln file and stop.

## 2. Find the dotnet runtime

Check if these paths exist (use Bash `test -f`), stop at the first match:

**macOS:**
- `/usr/local/share/dotnet/dotnet`
- `/opt/homebrew/bin/dotnet`

**Linux:**
- `/usr/share/dotnet/dotnet`
- `/snap/dotnet-sdk/current/dotnet`
- `$HOME/.dotnet/dotnet`

**Windows:**
- `C:\Program Files\dotnet\dotnet.exe`
- `%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`

**Fallback (all platforms):** run `which dotnet` or `where dotnet`.

If dotnet is not found anywhere, tell the user to install it from https://dotnet.microsoft.com/download and stop.

## 3. Find the plugin DLL path

Look for the OmniSharpMCP.dll inside `~/.claude/plugins/cache/beachbum-marketplace/omnisharp-mcp/`. Use Glob to find the exact path: `~/.claude/plugins/cache/beachbum-marketplace/omnisharp-mcp/*/publish/OmniSharpMCP.dll`.

If the DLL is not found, tell the user the omnisharp-mcp plugin may not be installed and stop.

## 4. Register the MCP server

Run this command (substitute the actual paths found above):

```
claude mcp add -s project -e OMNISHARP_SOLUTION=<absolute-sln-path> -- csharp <absolute-dotnet-path> <absolute-dll-path>
```

## 5. Confirm

Tell the user the C# MCP server is configured and they should restart Claude Code to activate it.
