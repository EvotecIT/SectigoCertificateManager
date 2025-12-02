Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

# Demonstrates renewing a certificate.
# Use the Admin path with -CertificateId when connected via ClientId/ClientSecret,
# or the legacy path with -OrderNumber when connected via username/password.

# Admin connection (OAuth2)
Connect-Sectigo -ClientId "client-id" `
                -ClientSecret "client-secret" `
                -Instance "enterprise" `
                -AdminBaseUrl "https://admin.enterprise.sectigo.com" | Out-Null

# Renew by certificate id (Admin API)
Invoke-SectigoCertificateRenewal -CertificateId 17331734 `
    -Csr (Get-Content "$PSScriptRoot/sample.csr" -Raw) `
    -DcvMode Email `
    -DcvEmail "admin@example.com"

# Legacy connection (username/password) - uncomment if you need legacy renew by order number
# Connect-Sectigo -BaseUrl "https://cert-manager.com/api" `
#                 -Username "user" `
#                 -Password "pass" `
#                 -CustomerUri "cst1" `
#                 -ApiVersion V25_6 | Out-Null
# Invoke-SectigoCertificateRenewal -OrderNumber 10 -Csr "CSR" -DcvMode Email -DcvEmail "admin@example.com"
