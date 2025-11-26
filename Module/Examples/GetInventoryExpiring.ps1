Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

$connectSectigoSplat = @{
    BaseUrl     = 'https://cert-manager.com/api'
    Username    = 'your-username'
    Password    = 'your-password'
    CustomerUri = 'your-customer-uri'
    ApiVersion  = 'V25_5'
}

Connect-Sectigo @connectSectigoSplat | Out-Null

$to = (Get-Date).Date.AddDays(30)
Write-Host 'Certificates expiring in the next 30 days:' -ForegroundColor Cyan
Get-SectigoInventory -DateTo $to |
    Select-Object Id, CommonName, Status, Expires |
    Format-Table -AutoSize
