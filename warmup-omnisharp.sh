#!/bin/bash
# Warmup script - starts OmniSharp and waits until it's ready
# Run this before starting Claude Code for instant tool availability

SOLUTION_PATH="${OMNISHARP_SOLUTION:-/Users/omersomekhbeachbum/dev/rummystars-client/rummystars-client.sln}"
PORT="${OMNISHARP_PORT:-2050}"
OMNISHARP_DLL="$HOME/.omnisharp-mcp/omnisharp/OmniSharp.dll"

# Check if already running
if curl -s -X POST "http://localhost:$PORT/checkreadystatus" -d '{}' 2>/dev/null | grep -q '"Ready":true'; then
    echo "OmniSharp is already running and ready on port $PORT"
    exit 0
fi

# Check if OmniSharp is installed
if [ ! -f "$OMNISHARP_DLL" ]; then
    echo "OmniSharp not found at $OMNISHARP_DLL"
    echo "Run the MCP server once to auto-download OmniSharp"
    exit 1
fi

echo "Starting OmniSharp for solution: $SOLUTION_PATH"
echo "Port: $PORT"

# Start OmniSharp in background
nohup dotnet "$OMNISHARP_DLL" -s "$SOLUTION_PATH" -p $PORT --hostPID $$ --encoding utf-8 > /tmp/omnisharp.log 2>&1 &
OMNISHARP_PID=$!

echo "OmniSharp started (PID: $OMNISHARP_PID)"
echo "Waiting for OmniSharp to become ready..."

# Wait for ready (up to 3 minutes)
for i in {1..180}; do
    if curl -s -X POST "http://localhost:$PORT/checkreadystatus" -d '{}' 2>/dev/null | grep -q '"Ready":true'; then
        echo "OmniSharp is ready!"
        exit 0
    fi
    sleep 1
    printf "."
done

echo ""
echo "Timeout waiting for OmniSharp. Check /tmp/omnisharp.log for errors."
exit 1
