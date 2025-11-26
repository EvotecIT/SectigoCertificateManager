Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

$connectSectigoSplat = @{
    BaseUrl     = 'https://cert-manager.com/api'
    Username    = 'your-username'
    Password    = 'your-password'
    CustomerUri = 'your-customer-uri'
    ApiVersion  = 'V25_5'
}

Connect-Sectigo @connectSectigoSplat | Out-Null

$from = (Get-Date).Date.AddDays(-7)
$to   = (Get-Date).Date
Write-Host "Certificates from $($from.ToString('yyyy-MM-dd')) to $($to.ToString('yyyy-MM-dd')):" -ForegroundColor Cyan
Get-SectigoInventory -DateFrom $from -DateTo $to |
    Select-Object Id, CommonName, Status, Expires |
    Format-Table -AutoSize
