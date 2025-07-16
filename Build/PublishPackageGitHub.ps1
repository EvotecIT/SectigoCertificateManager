$GitHubAccessToken = Get-Content -Raw 'C:\Support\Important\GithubAPI.txt'

$publishGitHubReleaseAssetSplat = @{
    ProjectPath          = "$PSScriptRoot\..\SectigoCertificateManager"
    GitHubAccessToken    = $GitHubAccessToken
    GitHubUsername       = "EvotecIT"
    GitHubRepositoryName = "SectigoCertificateManager"
    IsPreRelease         = $false
}

Publish-GitHubReleaseAsset @publishGitHubReleaseAssetSplat
