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

2. **Get all commits between releases** — Use `git log <prev_tag>..${{ inputs.version }} --oneline` via shell (preferred, since `list_commits` MCP tool is often filtered by integrity policy). This is the primary source of changes. Many important changes are direct commits without PRs — **do not skip them**.

3. **Understand what changed** — For commits that look significant (new features, bug fixes), use `git show <sha> --stat` or read the changed files to understand the scope. Look at commit messages carefully — they describe the actual changes.

4. **Get merged PRs** — Search for merged pull requests on the `master` branch between the two releases. Use the PR author and PR number for attribution. For commits without PRs, use the commit author and commit SHA.

**IMPORTANT**: Compare against the last **published** release (e.g., v0.12.152), NOT the immediately previous tag. Many tags may be CI/infrastructure-only. The commit list is the authoritative source — PRs supplement it but many changes are direct commits.

### 2. Categorize & Prioritize

Group changes by category (omit categories with no items):
- **⚠️ Breaking Changes** - Requires user action (ALWAYS list first if present)
- **✨ New Features** - User-facing capabilities
- **🐛 Bug Fixes** - Issue resolutions
- **🌐 Localization** - New or updated translations
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
- **Feature name** — short description. (#PR by @author)

### 🐛 Bug Fixes & Improvements
- **Fix name** — short description. (#PR by @author)

### 🌐 Localization
- Updated translations for [languages]. (#PR by @author)

**Full Changelog**: https://github.com/${{ github.repository }}/compare/<prev_tag>...${{ inputs.version }}
```

**Writing Guidelines:**
- **MAXIMUM 10-15 words per item description** — e.g. "In-place self-update with SHA256 verification and automatic rollback."
- Do NOT write multiple sentences per item. One short phrase only.
- Do NOT explain how features work or provide implementation details
- **Stay faithful to commit messages** — do not embellish, infer, or add details not in the commit message. If the commit says "fix X", describe it as fixing X, not what you think it might do.
- For each item, include the PR number and the **actual PR author** (check the PR data, not the commit author — they may differ)
- If a change has multiple PRs, list all PR numbers: `(#42 #43 by @author)`
- End with a Full Changelog link comparing the previous published release tag to `${{ inputs.version }}`
- Skip the summary paragraph if there are fewer than 5 user-facing changes

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
