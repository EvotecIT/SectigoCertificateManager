Describe "New-SectigoOrganization" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Release | Out-Null
        $dll = Join-Path $PSScriptRoot '../../SectigoCertificateManager.PowerShell/bin/Release/net8.0/SectigoCertificateManager.PowerShell.dll'
        Import-Module $dll
    }

    It "exports the cmdlet" {
        $cmd = Get-Command New-SectigoOrganization -ErrorAction Stop
        $cmd | Should -Not -BeNullOrEmpty
    }

    It "throws when Name is empty" {
        { New-SectigoOrganization -BaseUrl 'b' -Username 'u' -Password 'p' -CustomerUri 'c' -Name '' } | Should -Throw
    }
}
