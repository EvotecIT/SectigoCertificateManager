Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

$connectSectigoSplat = @{
    BaseUrl     = 'https://cert-manager.com/api'
    Username    = 'your-username'
    Password    = 'your-password'
    CustomerUri = 'your-customer-uri'
    ApiVersion  = 'V25_5'
}

Connect-Sectigo @connectSectigoSplat | Out-Null

Write-Host 'Latest 30 certificates:' -ForegroundColor Cyan
Get-SectigoInventory -Size 30 |
    Select-Object Id, CommonName, Status, Expires |
    Format-Table -AutoSize
