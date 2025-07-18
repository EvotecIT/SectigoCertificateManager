Describe "Remove-SectigoCertificate" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Release | Out-Null
        $dll = Join-Path $PSScriptRoot '../../SectigoCertificateManager.PowerShell/bin/Release/net8.0/SectigoCertificateManager.PowerShell.dll'
        Import-Module $dll
    }

    It "throws when CertificateId is less than or equal to zero" {
        { Remove-SectigoCertificate -BaseUrl 'b' -Username 'u' -Password 'p' -CustomerUri 'c' -CertificateId 0 } | Should -Throw
    }

    It "supports ShouldProcess" {
        $cmd = Get-Command Remove-SectigoCertificate -ErrorAction Stop
        $meta = [System.Management.Automation.CommandMetadata]::new($cmd.ImplementingType)
        $meta.SupportsShouldProcess | Should -BeTrue
    }
}
