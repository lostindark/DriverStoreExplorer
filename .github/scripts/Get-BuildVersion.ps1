param(
    [string]$Revision = 'HEAD',
    [string]$PullRequestNumber,
    [string]$HeadSha
)

$ErrorActionPreference = 'Stop'
$historyRevision = $Revision
if ($PullRequestNumber) {
    if ($PullRequestNumber -notmatch '^[1-9]\d*$' -or $HeadSha -notmatch '^[0-9a-f]{40}$') {
        throw 'A PR version requires a valid PR number and head commit SHA'
    }
    $mergeHead = git rev-parse --verify "${Revision}^2"
    if ($LASTEXITCODE -ne 0 -or $mergeHead -ne $HeadSha) {
        throw 'The PR checkout must be a merge of the expected head commit'
    }
    $historyRevision = "${Revision}^1"
}

$history = @(git log --first-parent --format='%H %D' --decorate=short --decorate-refs=refs/tags $historyRevision --)
if ($LASTEXITCODE -ne 0) { throw 'Failed to read first-parent version history' }

for ($distance = 0; $distance -lt $history.Count; $distance++) {
    $tags = [regex]::Matches($history[$distance], '(?: |, )tag: v(\d+\.\d+\.\d+)(?=,|$)')
    if ($tags.Count -eq 0) { continue }
    $baseVersion = $tags | ForEach-Object { [version]$_.Groups[1].Value } |
        Sort-Object -Descending | Select-Object -First 1
    $version = "$($baseVersion.Major).$($baseVersion.Minor).$($baseVersion.Build + $distance)"
    $fullVersion = $version
    if ($PullRequestNumber) {
        $fullVersion = "$version-pr.$PullRequestNumber+$($HeadSha.Substring(0, 8))"
    }
    return [PSCustomObject]@{
        Version = $version
        FullVersion = $fullVersion
    }
}

throw 'No numeric version tag found on the first-parent history'
