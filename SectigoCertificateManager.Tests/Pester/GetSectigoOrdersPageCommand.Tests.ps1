Describe "Get-SectigoOrdersPage" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Release | Out-Null
        $dll = Join-Path $PSScriptRoot '../../SectigoCertificateManager.PowerShell/bin/Release/net8.0/SectigoCertificateManager.PowerShell.dll'
        Import-Module $dll
    }

    It "exports the cmdlet" {
        $cmd = Get-Command Get-SectigoOrdersPage -ErrorAction Stop
        $cmd | Should -Not -BeNullOrEmpty
    }

    It "inherits from AsyncPSCmdlet" {
        $cmd = Get-Command Get-SectigoOrdersPage -ErrorAction Stop
        $cmd.ImplementingType.BaseType.FullName | Should -Be 'SectigoCertificateManager.PowerShell.AsyncPSCmdlet'
    }
}
