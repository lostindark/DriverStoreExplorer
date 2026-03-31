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
[Key features with user benefit — include author and PR link]

### 🐛 Bug Fixes & Improvements
[Notable fixes — include author and PR link]
```

**Writing Guidelines:**
- Lead with benefits: "Driver deletion is now 2x faster" not "Optimized delete loop"
- Be specific about what changed and why it matters to users
- Keep it concise and scannable (users grasp key changes in 30 seconds)
- Use professional, enthusiastic tone
- This is a Windows desktop application — write from the end-user perspective
- For each item, include the author and PR reference at the end, e.g.:
  `- **In-place self-update** — description. (#42 by @username)`

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

**CRITICAL**: Write the highlights to `/tmp/gh-aw/agent/release-highlights.md` using shell. This path is automatically collected as a workflow artifact.

```bash
cat > /tmp/gh-aw/agent/release-highlights.md << 'HIGHLIGHTS_EOF'
## 🌟 Release Highlights

[Your complete markdown highlights here]
HIGHLIGHTS_EOF
```

After writing the file, call `noop` and stop:
```
safeoutputs/noop(message="Release highlights saved to /tmp/gh-aw/agent/release-highlights.md")
```

**❌ DO NOT:**
- Call any tool after noop
- Investigate how safe outputs work internally
- Explore the workflow's lock.yml or CJS files
