Describe "New-SectigoOrder import via CLI" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Release | Out-Null
        $dll = Join-Path $PSScriptRoot '../../SectigoCertificateManager.PowerShell/bin/Release/net8.0/SectigoCertificateManager.PowerShell.dll'
        $cli = if (Get-Command pwsh -ErrorAction SilentlyContinue) { 'pwsh' } else { 'powershell' }
        $scriptContent = @"
Import-Module '$dll' -ErrorAction Stop
if (Get-Command New-SectigoOrder -ErrorAction SilentlyContinue) { exit 0 } else { exit 1 }
"@
        $tempFile = New-TemporaryFile
        Set-Content -Path $tempFile -Value $scriptContent
        & $cli -NoProfile -File $tempFile
        $Script:ImportExitCode = $LASTEXITCODE
    }

    It "imports module and finds New-SectigoOrder" {
        $ImportExitCode | Should -Be 0
    }
}
