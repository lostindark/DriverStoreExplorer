---
name: Release Highlights
description: Generate AI-powered release highlights for a release
on:
  workflow_dispatch:
    inputs:
      version:
        description: 'Release version tag (e.g. v1.0.0)'
        required: true
        type: string
permissions:
  contents: read
  pull-requests: read
engine: copilot
timeout-minutes: 10
network:
  allowed:
    - defaults
safe-outputs:
  jobs:
    save-highlights:
      description: 'Save release highlights to step summary and artifact'
      runs-on: ubuntu-latest
      permissions:
        contents: read
      steps:
        - name: Save highlights
          run: |
            # Try multiple possible paths in agent output
            HIGHLIGHTS=$(jq -r '
              .items[0].highlights //
              .items[0].body //
              .items[0].content //
              (.items[] | select(.type == "save_highlights") | .highlights) //
              empty
            ' "$GH_AW_AGENT_OUTPUT" 2>/dev/null || true)
            if [ -z "$HIGHLIGHTS" ]; then
              echo "::warning::No highlights found in agent output, dumping structure:"
              jq '.' "$GH_AW_AGENT_OUTPUT" 2>/dev/null || cat "$GH_AW_AGENT_OUTPUT"
              exit 1
            fi
            echo "$HIGHLIGHTS" >> "$GITHUB_STEP_SUMMARY"
            echo "$HIGHLIGHTS" > release-highlights.md
        - name: Upload artifact
          uses: actions/upload-artifact@v4
          with:
            name: release-highlights
            path: release-highlights.md
---

# Release Highlights Generator

Generate an engaging release highlights summary for **${{ github.repository }}** release `${{ inputs.version }}`.

## Workflow

### 1. Gather Release Data

Use the GitHub MCP tools to fetch release information for `${{ github.repository }}`:

1. **Find the previous published release** — List releases for the repository. Find the most recent release that is **not a draft** and is **published** (i.e., has a `published_at` date). This is the baseline to compare against. Note its tag name.

2. **Get all commits between releases** — Use `list_commits` or `git log <prev_tag>..${{ inputs.version }} --oneline` via shell to get all commits between the previous published release tag and `${{ inputs.version }}`.

3. **Optionally get merged PRs** — Search for merged pull requests if needed for additional context, but rely primarily on commits since not all changes go through PRs. Use the default branch (`master`), not `main`.

**IMPORTANT**: Compare against the last **published** release (e.g., v0.12.152), NOT the immediately previous tag. Many tags may be CI/infrastructure-only.

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

### 5. Save Highlights

**CRITICAL**: You MUST call the `save_highlights` tool to save the generated highlights. After calling it successfully, **STOP immediately**. Do not investigate the workflow internals or try to verify how the tool works.

**✅ CORRECT - Call the tool directly:**
```
safeoutputs/save_highlights(
  highlights="## 🌟 Release Highlights\n\n[Your complete markdown highlights here]"
)
```

After calling `save_highlights`, your job is done. Stop.

**❌ INCORRECT - DO NOT:**
- Investigate how safe outputs work internally
- Write JSON files manually
- Use bash to simulate tool calls
- Explore the workflow's lock.yml or CJS files

**Important**: If no action is needed after completing your analysis, you **MUST** call the `noop` safe-output tool:
```
safeoutputs/noop(message="No action needed: [brief explanation]")
```
