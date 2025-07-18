Describe "Update-SectigoCertificate" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Release | Out-Null
        $dll = Join-Path $PSScriptRoot '../../SectigoCertificateManager.PowerShell/bin/Release/net8.0/SectigoCertificateManager.PowerShell.dll'
        Import-Module $dll
    }

    It "exports the cmdlet" {
        $cmd = Get-Command Update-SectigoCertificate -ErrorAction Stop
        $cmd | Should -Not -BeNullOrEmpty
    }

    It "supports ShouldProcess" {
        $cmd = Get-Command Update-SectigoCertificate -ErrorAction Stop
        $meta = [System.Management.Automation.CommandMetadata]::new($cmd.ImplementingType)
        $meta.SupportsShouldProcess | Should -BeTrue
    }
}
