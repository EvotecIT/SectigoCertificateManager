Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

# Demonstrates exporting a certificate (Admin and legacy).

# Admin (OAuth2)
Connect-Sectigo -ClientId "client-id" `
                -ClientSecret "client-secret" `
                -Instance "enterprise" `
                -AdminBaseUrl "https://admin.enterprise.sectigo.com" | Out-Null

# PEM
Export-SectigoCertificate -CertificateId 17331734 -Path "cert.pem" -Format Pem

# PFX (set a password)
Export-SectigoCertificate -CertificateId 17331734 `
    -Format Pfx `
    -PfxPassword (Read-Host -AsSecureString "Pfx password") `
    -Path "cert.pfx"

# Legacy (username/password) - uncomment if needed
# Connect-Sectigo -BaseUrl "https://cert-manager.com/api" `
#                 -Username "user" `
#                 -Password "pass" `
#                 -CustomerUri "cst1" `
#                 -ApiVersion V25_6 | Out-Null
# Export-SectigoCertificate -CertificateId 10 -Path "cert.pem" -Format Pem
