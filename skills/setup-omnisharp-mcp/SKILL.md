---
name: setup-omnisharp-mcp
description: >
  This skill should be used when the user asks to "setup C# MCP",
  "configure OmniSharp", "setup the .sln server", or when the C# MCP
  server fails to start because dotnet is not found or no .sln file
  was detected.
---

Set up the omnisharp-mcp server for this project. Follow these steps in order, stopping early if any step fails.

## Step 1 -- Find the .sln file

Search for `**/*.sln` in the current working directory.

- If **multiple** `.sln` files are found, ask the user which one to use.
- If **none** are found, tell the user: "No .sln file found in this project. The C# MCP server requires a Visual Studio solution file." Then **stop**.

Store the **absolute path** to the chosen `.sln` file for later.

## Step 2 -- Find the dotnet runtime

Check platform-specific paths in order, stopping at the first one that exists.

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

**Fallback (all platforms):** run `which dotnet` (or `where dotnet` on Windows).

If an absolute path is found, store it for later.

If dotnet is not found anywhere, fall back to plain `dotnet` and warn the user:
"dotnet was not found at any known location. Falling back to `dotnet` and assuming it is on your PATH. If the MCP server fails to start, install the .NET SDK from https://dotnet.microsoft.com/download and ensure `dotnet` is on your PATH."

## Step 3 -- Find the plugin DLL

Search for the OmniSharpMCP.dll inside the plugin cache:

```
~/.claude/plugins/cache/beachbum-marketplace/omnisharp-mcp/*/publish/OmniSharpMCP.dll
```

If the DLL is not found, tell the user:
"Could not find OmniSharpMCP.dll. The omnisharp-mcp plugin may not be installed correctly. Try reinstalling the plugin."
Then **stop**.

Store the **absolute path** to the DLL for later.

## Step 4 -- Register the MCP server

Run the following command, substituting the actual absolute paths found in the previous steps:

```
claude mcp add -s project -e OMNISHARP_SOLUTION=<absolute-sln-path> -- csharp <dotnet-path-or-dotnet> <absolute-dll-path>
```

Where `<dotnet-path-or-dotnet>` is the absolute path found in Step 2, or plain `dotnet` if no absolute path was found.

If the command fails, show the error to the user and **stop**.

## Step 5 -- Confirm

Tell the user: "The C# MCP server is now configured for this project. Please restart Claude Code to activate the new MCP server."
