$ErrorActionPreference = 'Stop'
$versionScript = Join-Path $PSScriptRoot 'Get-BuildVersion.ps1'
$repository = Join-Path ([System.IO.Path]::GetTempPath()) "rapr-version-tests-$([guid]::NewGuid())"

function Invoke-TestGit {
    & git @args
    if ($LASTEXITCODE -ne 0) { throw "Git test setup failed: $args" }
}

function Assert-Version([string]$Version, [string]$FullVersion = $Version, [hashtable]$Parameters = @{}) {
    $actual = & $versionScript @Parameters
    if ($actual.Version -ne $Version -or $actual.FullVersion -ne $FullVersion) {
        throw "Expected $Version / $FullVersion; got $($actual.Version) / $($actual.FullVersion)"
    }
}

function Assert-Throws([scriptblock]$Action, [string]$Message) {
    try {
        & $Action | Out-Null
    } catch {
        if ($_.Exception.Message -notlike $Message) { throw }
        return
    }
    throw "Expected error: $Message"
}

New-Item -ItemType Directory -Path $repository | Out-Null
Push-Location $repository
try {
    Invoke-TestGit init --quiet --initial-branch=master
    Invoke-TestGit config user.name 'Version Tests'
    Invoke-TestGit config user.email 'version-tests@example.invalid'
    Invoke-TestGit config commit.gpgsign false
    Invoke-TestGit config tag.gpgsign false
    Invoke-TestGit commit --quiet --allow-empty -m 'Initial commit'
    Assert-Throws { & $versionScript } 'No numeric version tag found*'

    Invoke-TestGit tag v1.0.72
    Assert-Version '1.0.72'
    Invoke-TestGit commit --quiet --allow-empty -m 'First master commit'
    Invoke-TestGit tag v9.0.0-preview
    Assert-Version '1.0.73'
    Invoke-TestGit commit --quiet --allow-empty -m 'Second master commit'
    Assert-Version '1.0.74'

    Invoke-TestGit tag v1.0.74
    Invoke-TestGit tag v1.0.9
    Assert-Version '1.0.74'
    Assert-Version '1.0.73' -Parameters @{ Revision = 'HEAD^1' }

    Invoke-TestGit checkout --quiet -b feature
    Invoke-TestGit commit --quiet --allow-empty -m 'First feature commit'
    Invoke-TestGit tag v9.0.0
    Invoke-TestGit commit --quiet --allow-empty -m 'Second feature commit'
    $headSha = Invoke-TestGit rev-parse HEAD
    Invoke-TestGit checkout --quiet master
    Invoke-TestGit merge --quiet --no-ff feature -m 'Merge feature'
    Assert-Version '1.0.75'
    Assert-Version '1.0.74' "1.0.74-pr.510+$($headSha.Substring(0, 8))" @{
        PullRequestNumber = '510'
        HeadSha = $headSha
    }
    Assert-Throws {
        & $versionScript -PullRequestNumber '510' -HeadSha ('0' * 40)
    } 'The PR checkout must be a merge of the expected head commit'

    Invoke-TestGit tag -a v1.0.75 -m 'Version 1.0.75'
    Assert-Version '1.0.75'
    Write-Host 'Build version tests passed'
} finally {
    Pop-Location
    Remove-Item -LiteralPath $repository -Recurse -Force
}
