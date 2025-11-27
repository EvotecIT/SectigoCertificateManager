Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

# Demonstrates retrieving certificate status.
Connect-Sectigo -BaseUrl "https://example.com" `
                -Username "user" `
                -Password "pass" `
                -CustomerUri "cst1" `
                -ApiVersion V25_6 | Out-Null

Get-SectigoCertificateStatus -CertificateId 10

