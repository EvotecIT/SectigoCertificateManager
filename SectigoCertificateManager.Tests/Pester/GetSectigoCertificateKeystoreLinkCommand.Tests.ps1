Describe "Get-SectigoCertificateKeystoreLink" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Release | Out-Null
        $dll = Join-Path $PSScriptRoot '../../SectigoCertificateManager.PowerShell/bin/Release/net8.0/SectigoCertificateManager.PowerShell.dll'
        Import-Module $dll
    }

    It "exports the cmdlet" {
        $cmd = Get-Command Get-SectigoCertificateKeystoreLink -ErrorAction Stop
        $cmd | Should -Not -BeNullOrEmpty
    }

    It "throws when CertificateId is less than or equal to zero" {
        { Get-SectigoCertificateKeystoreLink -CertificateId 0 -FormatType 'p12' } | Should -Throw -ErrorId 'InvalidCertificateId,SectigoCertificateManager.PowerShell.GetSectigoCertificateKeystoreLinkCommand'
    }
}

