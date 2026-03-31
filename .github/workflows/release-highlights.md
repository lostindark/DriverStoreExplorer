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
  - name: Setup release data
    env:
      GH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
      WORKFLOW_CONCLUSION: ${{ github.event.workflow_run.conclusion }}
    run: |
      set -e

      # Only proceed if the triggering workflow succeeded
      if [ "$WORKFLOW_CONCLUSION" != "success" ]; then
        echo "Release workflow did not succeed. Skipping."
        exit 1
      fi

      mkdir -p /tmp/release-data

      # Find the latest draft release
      gh api "repos/${{ github.repository }}/releases" \
        --jq '[.[] | select(.draft)] | .[0]' > /tmp/release-data/current_release.json

      RELEASE_TAG=$(jq -r '.tag_name' /tmp/release-data/current_release.json)
      RELEASE_ID=$(jq -r '.id' /tmp/release-data/current_release.json)

      if [ "$RELEASE_TAG" = "null" ] || [ -z "$RELEASE_TAG" ]; then
        echo "No draft release found."
        exit 1
      fi

      echo "Draft release: $RELEASE_TAG (ID: $RELEASE_ID)"
      echo "RELEASE_TAG=$RELEASE_TAG" >> "$GITHUB_ENV"
      echo "RELEASE_ID=$RELEASE_ID" >> "$GITHUB_ENV"

      # Find previous non-draft release
      PREV_RELEASE_TAG=$(gh api "repos/${{ github.repository }}/releases" \
        --jq '[.[] | select(.draft | not)] | .[0].tag_name // empty')
      echo "PREV_RELEASE_TAG=$PREV_RELEASE_TAG" >> "$GITHUB_ENV"

      if [ -n "$PREV_RELEASE_TAG" ]; then
        echo "Previous release: $PREV_RELEASE_TAG"

        # Get commits between tags via API
        gh api "repos/${{ github.repository }}/compare/${PREV_RELEASE_TAG}...${RELEASE_TAG}" \
          --jq '.commits | [.[] | {sha: .sha[:8], message: (.commit.message | split("\n") | .[0]), author: .author.login}]' \
          > /tmp/release-data/commits.json

        COMMIT_COUNT=$(jq length /tmp/release-data/commits.json)
        echo "✓ Found $COMMIT_COUNT commits"

        # Get merged PRs between releases
        PREV_DATE=$(gh api "repos/${{ github.repository }}/releases/tags/${PREV_RELEASE_TAG}" --jq '.published_at')
        CURR_DATE=$(jq -r '.created_at' /tmp/release-data/current_release.json)

        gh pr list \
          --state merged \
          --limit 100 \
          --json number,title,author,labels,mergedAt,url \
          --jq "[.[] | select(.mergedAt >= \"$PREV_DATE\" and .mergedAt <= \"$CURR_DATE\")]" \
          > /tmp/release-data/pull_requests.json

        PR_COUNT=$(jq length /tmp/release-data/pull_requests.json)
        echo "✓ Found $PR_COUNT pull requests"
      else
        echo "No previous release found. This is the first release."
        echo "[]" > /tmp/release-data/commits.json
        echo "[]" > /tmp/release-data/pull_requests.json
      fi

      echo "✓ Setup complete"
---

# Release Highlights Generator

Generate an engaging release highlights summary for **${{ github.repository }}** release `${RELEASE_TAG}`.

## Data Available

All data is pre-fetched in `/tmp/release-data/`:
- `current_release.json` - Draft release metadata (tag, name, existing body with auto-generated notes)
- `commits.json` - Commits since `${PREV_RELEASE_TAG}` (sha, message, author)
- `pull_requests.json` - Merged PRs between releases (may be empty if changes were direct commits)

## Workflow

### 1. Load Data

```bash
# View release metadata
cat /tmp/release-data/current_release.json | jq '{tag_name, name, created_at}'

# List commits
cat /tmp/release-data/commits.json | jq -r '.[] | "- \(.message) (\(.sha)) by @\(.author)"'

# List PRs (may be empty)
cat /tmp/release-data/pull_requests.json | jq -r '.[] | "- #\(.number): \(.title) by @\(.author.login)"'
```

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

**First Release** (no `${PREV_RELEASE_TAG}`):
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
  tag="${RELEASE_TAG}",
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
