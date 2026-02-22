#!/usr/bin/env node
"use strict";

const { spawn, execFileSync } = require("child_process");
const path = require("path");
const fs = require("fs");
const os = require("os");

function findDotnet() {
  const platform = process.platform;
  const candidates = [];

  if (platform === "darwin") {
    candidates.push("/usr/local/share/dotnet/dotnet");
    candidates.push("/opt/homebrew/bin/dotnet");
  } else if (platform === "win32") {
    const programFiles = process.env.ProgramFiles || "C:\\Program Files";
    const localAppData = process.env.LOCALAPPDATA || "";
    candidates.push(path.join(programFiles, "dotnet", "dotnet.exe"));
    if (localAppData) {
      candidates.push(path.join(localAppData, "Microsoft", "dotnet", "dotnet.exe"));
    }
    candidates.push("C:\\Program Files\\dotnet\\dotnet.exe");
  } else {
    candidates.push("/usr/share/dotnet/dotnet");
    candidates.push("/snap/dotnet-sdk/current/dotnet");
    const home = os.homedir();
    if (home) {
      candidates.push(path.join(home, ".dotnet", "dotnet"));
    }
  }

  for (const candidate of candidates) {
    if (fs.existsSync(candidate)) {
      return candidate;
    }
  }

  // Fallback: try to find dotnet on PATH
  try {
    const cmd = platform === "win32" ? "where" : "which";
    const result = execFileSync(cmd, ["dotnet"], {
      encoding: "utf8",
      stdio: ["pipe", "pipe", "pipe"],
    });
    const found = result.trim().split(/\r?\n/)[0];
    if (found && fs.existsSync(found)) {
      return found;
    }
  } catch {
    // not on PATH
  }

  return null;
}

const dotnet = findDotnet();
if (!dotnet) {
  process.stderr.write(
    "Error: dotnet runtime not found.\n" +
    "Install the .NET SDK from https://dotnet.microsoft.com/download\n"
  );
  process.exit(1);
}

const scriptDir = __dirname;
const dllPath = path.join(scriptDir, "publish", "OmniSharpMCP.dll");

const child = spawn(dotnet, [dllPath], {
  stdio: "inherit",
  cwd: scriptDir,
  env: process.env,
});

child.on("error", (err) => {
  process.stderr.write(`Failed to start dotnet: ${err.message}\n`);
  process.exit(1);
});

child.on("exit", (code) => {
  process.exit(code ?? 1);
});
