# Run with gh authenticated as an administrator of Pedroltz/ofizzy.
$ErrorActionPreference = 'Stop'
$repo = 'repos/Pedroltz/ofizzy'
$outputDirectory = Join-Path $PSScriptRoot '../test-results/github-demo'
New-Item -ItemType Directory -Force $outputDirectory | Out-Null

function Invoke-GitHubJson([string] $Method, [string] $Endpoint, $Body) {
    $payloadPath = Join-Path $outputDirectory 'github-payload.json'
    [System.IO.File]::WriteAllText($payloadPath, ($Body | ConvertTo-Json -Depth 10), (New-Object System.Text.UTF8Encoding $false))
    gh api --method $Method $Endpoint --input $payloadPath
    if ($LASTEXITCODE -ne 0) { throw "GitHub API failed: $Method $Endpoint" }
}

$metadata = gh api $repo | ConvertFrom-Json
if ($LASTEXITCODE -ne 0 -or !$metadata.permissions.admin) {
    throw 'An administrator account is required; no configuration changed.'
}

Invoke-GitHubJson PATCH $repo @{
    description = 'Gestão SaaS multi-tenant para prestadores de serviços e oficinas: clientes, veículos, ordens de serviço, recebimentos e dashboard. Angular, .NET e PostgreSQL.'
    default_branch = 'develop'
    has_issues = $true
    has_wiki = $false
    allow_merge_commit = $true
    allow_squash_merge = $true
    allow_rebase_merge = $false
    delete_branch_on_merge = $false
    security_and_analysis = @{
        secret_scanning = @{ status = 'enabled' }
        secret_scanning_push_protection = @{ status = 'enabled' }
    }
} | Out-Null

Invoke-GitHubJson PUT "$repo/topics" @{
    names = @('angular', 'aspnetcore', 'automotive', 'csharp', 'docker', 'dotnet', 'efcore', 'multi-tenant', 'postgresql', 'primeng', 'saas', 'tailwind', 'work-order')
} | Out-Null

foreach ($feature in @('vulnerability-alerts', 'automated-security-fixes', 'private-vulnerability-reporting')) {
    gh api --method PUT "$repo/$feature"
    if ($LASTEXITCODE -ne 0) { throw "Could not enable $feature" }
}

Invoke-GitHubJson PUT "$repo/branches/main/protection" @{
    required_status_checks = @{ strict = $true; contexts = @('backend', 'frontend', 'compose') }
    enforce_admins = $true
    required_pull_request_reviews = @{ dismiss_stale_reviews = $true; require_code_owner_reviews = $false; required_approving_review_count = 0 }
    restrictions = $null
    required_conversation_resolution = $true
    allow_force_pushes = $false
    allow_deletions = $false
} | Out-Null

$result = @{
    verifiedAt = [DateTimeOffset]::UtcNow.ToString('o')
    repository = (gh api $repo | ConvertFrom-Json)
    mainProtection = (gh api "$repo/branches/main/protection" | ConvertFrom-Json)
    privateReporting = (gh api "$repo/private-vulnerability-reporting" | ConvertFrom-Json)
    topics = (gh api "$repo/topics" | ConvertFrom-Json)
}
if ($LASTEXITCODE -ne 0) { throw 'Failed to verify final configuration.' }
$result | ConvertTo-Json -Depth 15 | Set-Content -Encoding utf8 (Join-Path $outputDirectory 'github-settings-evidence.json')
Write-Output 'Configured: main PR + backend/frontend/compose checks; develop unchanged; secret scanning, push protection, dependency alerts/fixes and private reporting enabled.'
