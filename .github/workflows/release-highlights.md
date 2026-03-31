---
name: Release Highlights
description: Generate AI-powered release highlights and prepend to draft release
on:
  workflow_dispatch:
  workflow_run:
    workflows: ["Release"]
    types: [completed]
    branches:
      - master
permissions:
  contents: read
  pull-requests: read
engine: copilot
timeout-minutes: 10
network:
  allowed:
    - defaults
safe-outputs:
  update-release:
steps:
  - name: Check workflow conclusion
    env:
      WORKFLOW_CONCLUSION: ${{ github.event.workflow_run.conclusion }}
    run: |
      # Only proceed if the triggering workflow succeeded (skip check for manual dispatch)
      if [ -n "$WORKFLOW_CONCLUSION" ] && [ "$WORKFLOW_CONCLUSION" != "success" ]; then
        echo "Release workflow did not succeed. Skipping."
        exit 1
      fi
---

# Release Highlights Generator

Generate an engaging release highlights summary for **${{ github.repository }}**.

## Workflow

### 1. Gather Release Data

Use the GitHub MCP tools to fetch release information:

1. **Find the latest draft release** — List releases for `${{ github.repository }}` and find the first draft release. If no draft release exists, call `safeoutputs/noop(message="No draft release found")` and stop.

2. **Find the previous published release** — From the same releases list, find the most recent non-draft release. Note its tag name.

3. **Get commits between releases** — Use the GitHub MCP tools to compare the previous release tag with the draft release tag to get the list of commits.

4. **Get merged PRs** — Search for merged pull requests in the repository between the two releases.

### 2. Categorize & Prioritize

Group changes by category (omit categories with no items):
- **⚠️ Breaking Changes** - Requires user action (ALWAYS list first if present)
- **✨ New Features** - User-facing capabilities
- **🐛 Bug Fixes** - Issue resolutions
- **⚡ Performance** - Speed/efficiency improvements
- **🔧 Internal** - Refactoring, dependencies (usually omit from highlights)

Use both commit messages and PR titles to determine categories.
Skip internal/refactoring changes unless they have user impact.

### 3. Write Highlights

Structure:
```markdown
## 🌟 Release Highlights

[1-2 sentence summary of the release theme/focus]

### ⚠️ Breaking Changes
[If any - list FIRST with migration guidance]

### ✨ What's New
[Key features with user benefit]

### 🐛 Bug Fixes & Improvements
[Notable fixes - focus on user impact]
```

**Writing Guidelines:**
- Lead with benefits: "Driver deletion is now 2x faster" not "Optimized delete loop"
- Be specific about what changed and why it matters to users
- Keep it concise and scannable (users grasp key changes in 30 seconds)
- Use professional, enthusiastic tone
- This is a Windows desktop application — write from the end-user perspective

### 4. Handle Special Cases

**First Release** (no previous release):
```markdown
## 🎉 First Release
Welcome to the inaugural release! This version includes the following capabilities:
[List primary features]
```

**Maintenance Release** (no user-facing changes):
```markdown
## 🔧 Maintenance Release
Dependency updates and internal improvements to keep things running smoothly.
```

### 5. Update Release

**CRITICAL**: You MUST call the `update_release` MCP tool to prepend highlights to the draft release.

**✅ CORRECT - Call the MCP tool directly:**
```
safeoutputs/update_release(
  tag="<draft release tag>",
  operation="prepend",
  body="## 🌟 Release Highlights\n\n[Your complete markdown highlights here]"
)
```

**❌ INCORRECT - DO NOT:**
- Write JSON files manually
- Use bash to simulate tool calls
- Create scripts that write to outputs

**Important**: If no action is needed after completing your analysis, you **MUST** call the `noop` safe-output tool:
```
safeoutputs/noop(message="No action needed: [brief explanation]")
```
