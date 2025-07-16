Describe "Get-SectigoCertificates" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Debug | Out-Null
    }

    It "exports the cmdlet" {
        $cmd = Get-Command Get-SectigoCertificates -ErrorAction Stop
        $cmd | Should -Not -BeNullOrEmpty
    }
}
