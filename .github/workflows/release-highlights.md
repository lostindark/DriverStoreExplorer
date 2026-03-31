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
            HIGHLIGHTS=$(jq -r '.items[0].highlights // empty' "$GH_AW_AGENT_OUTPUT")
            if [ -z "$HIGHLIGHTS" ]; then
              echo "No highlights found in agent output"
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

1. **Get the list of releases** — List releases and find the tag `${{ inputs.version }}`.

2. **Find the previous published release** — From the releases list, find the most recent non-draft, published release before `${{ inputs.version }}`. Note its tag name.

3. **Get commits between releases** — Compare the previous release tag with `${{ inputs.version }}` to get the list of commits.

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

### 5. Save Highlights

**CRITICAL**: You MUST call the `save_highlights` tool to save the generated highlights.

**✅ CORRECT - Call the tool directly:**
```
safeoutputs/save_highlights(
  highlights="## 🌟 Release Highlights\n\n[Your complete markdown highlights here]"
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
