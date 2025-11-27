Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

# NOTE:
# Get-SectigoEnrollCertificates talks directly to the Enroll API and does not
# use Connect-Sectigo. Provide the Enroll endpoint URL and credentials here.

Write-Host 'Latest 30 certificates via Enroll API:' -ForegroundColor Cyan
Get-SectigoEnrollCertificates `
    -BaseUrl 'https://company.enroll.enterprise.sectigo.com' `
    -Username 'your-username' `
    -Password 'your-password' `
    -CustomerUri 'your-customer-uri' `
    -Size 30 |
    Format-Table -AutoSize
