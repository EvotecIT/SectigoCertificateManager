Describe "Stop-SectigoOrder" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Release | Out-Null
        $dll = Join-Path $PSScriptRoot '../../SectigoCertificateManager.PowerShell/bin/Release/net8.0/SectigoCertificateManager.PowerShell.dll'
        Import-Module $dll
    }

    It "throws when OrderId is less than or equal to zero" {
        { Stop-SectigoOrder -OrderId 0 } | Should -Throw -ErrorId 'InvalidOrderId,SectigoCertificateManager.PowerShell.StopSectigoOrderCommand'
    }

    It "supports ShouldProcess" {
        $cmd = Get-Command Stop-SectigoOrder -ErrorAction Stop
        $meta = [System.Management.Automation.CommandMetadata]::new($cmd.ImplementingType)
        $meta.SupportsShouldProcess | Should -BeTrue
    }
}
