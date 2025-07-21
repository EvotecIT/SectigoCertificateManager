Describe 'Get-SectigoToken behavior' -Tag 'Cmdlet' {
    It 'Returns nothing when token file is missing' {
        $temp = [System.IO.Path]::Combine([System.IO.Path]::GetTempPath(), [System.Guid]::NewGuid().ToString(), 'missing.json')
        $result = Get-SectigoToken -Path $temp
        $result | Should -BeNullOrEmpty
    }
}
