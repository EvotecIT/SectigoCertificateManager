Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

$connectSectigoSplat = @{
    BaseUrl     = 'https://company.enroll.enterprise.sectigo.com'
    Username    = 'your-username'
    Password    = 'your-password'
    CustomerUri = 'your-customer-uri'
}

Connect-Sectigo @connectSectigoSplat | Out-Null

Write-Host 'Latest 30 certificates via Enroll API:' -ForegroundColor Cyan
Get-SectigoEnrollCertificates -BaseUrl $connectSectigoSplat.BaseUrl -Size 30 |
    Format-Table -AutoSize
