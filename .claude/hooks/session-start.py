#!/usr/bin/env python3
"""SessionStart hook: prints a short orientation summary (branch, change count,
pointer to ai-context/) so a new/cleared session doesn't lose the thread.
Skipped on resume/compact - those already carry the conversation."""
import json
import os
import subprocess
import sys

try:
    data = json.load(sys.stdin)
except Exception:
    data = {}

if data.get("source") in ("resume", "compact"):
    sys.exit(0)

root = os.path.dirname(os.path.dirname(os.path.dirname(os.path.abspath(__file__))))

try:
    status = subprocess.run(
        ["git", "status", "-sb"],
        cwd=root, capture_output=True, text=True, timeout=5,
    ).stdout
except Exception:
    status = ""

lines = status.splitlines()
branch_line = lines[0][3:] if lines and lines[0].startswith("## ") else "unknown branch"
changes = max(len(lines) - 1, 0)

msg = (
    f"softgames-task orientation - {branch_line}, {changes} changed/untracked file(s). "
    "Read ai-context/current-context.md first (current state + immediate next step), "
    "then ai-context/decisions.md for the why behind past choices, before doing anything else."
)

print(json.dumps({
    "systemMessage": msg,
    "hookSpecificOutput": {
        "hookEventName": "SessionStart",
        "additionalContext": msg,
    },
}))
