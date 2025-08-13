Describe "Export-SectigoCertificate" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Release | Out-Null
        $dll = Join-Path $PSScriptRoot '../../SectigoCertificateManager.PowerShell/bin/Release/net8.0/SectigoCertificateManager.PowerShell.dll'
        Import-Module $dll
    }

    It "exports the cmdlet" {
        $cmd = Get-Command Export-SectigoCertificate -ErrorAction Stop
        $cmd | Should -Not -BeNullOrEmpty
    }

    It "throws when CertificateId is less than or equal to zero" {
        { Export-SectigoCertificate -BaseUrl 'b' -Username 'u' -Password 'p' -CustomerUri 'c' -CertificateId 0 -Path 'cert.pem' } | Should -Throw -ErrorId 'InvalidCertificateId,SectigoCertificateManager.PowerShell.ExportSectigoCertificateCommand'
    }

    It "throws when Path is null or empty" {
        { Export-SectigoCertificate -BaseUrl 'b' -Username 'u' -Password 'p' -CustomerUri 'c' -CertificateId 1 -Path '' } | Should -Throw -ErrorId 'InvalidPath,SectigoCertificateManager.PowerShell.ExportSectigoCertificateCommand'
    }
}

