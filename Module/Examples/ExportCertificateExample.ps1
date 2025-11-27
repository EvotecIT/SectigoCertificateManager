Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

# Demonstrates exporting a certificate.
Connect-Sectigo -BaseUrl "https://example.com" `
                -Username "user" `
                -Password "pass" `
                -CustomerUri "cst1" `
                -ApiVersion V25_6 | Out-Null

Export-SectigoCertificate -CertificateId 10 -Path "cert.pem" -Format Pem

