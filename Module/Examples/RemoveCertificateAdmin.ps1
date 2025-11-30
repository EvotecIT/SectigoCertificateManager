Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

$connectSectigoSplat = @{
    ClientId     = 'your-client-id'
    ClientSecret = 'your-client-secret'
    Instance     = 'enterprise'
    AdminBaseUrl = 'https://admin.enterprise.sectigo.com'
}

Connect-Sectigo @connectSectigoSplat | Out-Null

$certificateId = 17331734
Write-Host "Revoking certificate $certificateId via Admin API..." -ForegroundColor Cyan

Remove-SectigoCertificate -CertificateId $certificateId -ReasonCode KeyCompromise -Reason 'Key compromised'

