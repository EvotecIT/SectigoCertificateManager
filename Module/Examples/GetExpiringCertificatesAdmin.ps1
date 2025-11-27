Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

$connectSectigoSplat = @{
    ClientId     = 'your-client-id'
    ClientSecret = 'your-client-secret'
    Instance     = 'enterprise'
    AdminBaseUrl = 'https://admin.enterprise.sectigo.com'
}

Connect-Sectigo @connectSectigoSplat | Out-Null

$days = 30
Write-Host "Certificates (Status=Issued) expiring within the next $days days:" -ForegroundColor Cyan

$expiring = Get-SectigoCertificate -Status Issued -ExpiresWithinDays $days

$expiring |
    Select-Object Id, CommonName, Expires, Requester |
    Format-Table -AutoSize

