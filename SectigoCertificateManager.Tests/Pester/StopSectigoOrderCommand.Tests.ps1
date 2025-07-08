Describe "Stop-SectigoOrder" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Release | Out-Null
        $dll = Join-Path $PSScriptRoot '../../SectigoCertificateManager.PowerShell/bin/Release/net8.0/SectigoCertificateManager.PowerShell.dll'
        Import-Module $dll
    }

    It "exports the cmdlet" {
        $cmd = Get-Command Stop-SectigoOrder -ErrorAction Stop
        $cmd | Should -Not -BeNullOrEmpty
    }

    It "has the correct output type" {
        $cmd = Get-Command Stop-SectigoOrder -ErrorAction Stop
        ($cmd.OutputType | Select-Object -First 1).Name | Should -Be 'System.Void'
    }
}
