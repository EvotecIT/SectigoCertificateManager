Describe "Get-SectigoToken" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Release | Out-Null
        $dll = Join-Path $PSScriptRoot '../../SectigoCertificateManager.PowerShell/bin/Release/net8.0/SectigoCertificateManager.PowerShell.dll'
        Import-Module $dll
    }

    It "exports the cmdlet" {
        $cmd = Get-Command Get-SectigoToken -ErrorAction Stop
        $cmd | Should -Not -BeNullOrEmpty
    }

    It "reads a token file" {
        $tempDir = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [System.Guid]::NewGuid().ToString())
        [System.IO.Directory]::CreateDirectory($tempDir) | Out-Null
        $path = [System.IO.Path]::Combine($tempDir, 'token.json')
        $info = [SectigoCertificateManager.TokenInfo]::new('tok', [System.DateTimeOffset]::UtcNow.AddMinutes(5))
        [SectigoCertificateManager.ApiConfigLoader]::WriteToken($info, $path)
        $result = Get-SectigoToken -Path $path
        $result.Token | Should -Be 'tok'
        Remove-Item -Path $tempDir -Recurse -Force
    }
}
