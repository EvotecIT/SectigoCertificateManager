Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

# Legacy connection (ordering currently legacy-only)
Connect-Sectigo -BaseUrl "https://cert-manager.com/api" `
                -Username "user" `
                -Password "pass" `
                -CustomerUri "cst1" `
                -ApiVersion V25_6 | Out-Null

# Generate CSR for the new order
$san = @('new.example.com','www.new.example.com')
$csr = New-SectigoCsr -CommonName "new.example.com" -DnsName $san -Organization "Example" -Country "US"

# Place order with generated CSR
$order = New-SectigoOrder -CertificateType 501 -Term 365 -Csr $csr.Csr -SubjectAlternativeNames $san

Write-Host "Order placed: $($order.OrderId)" -ForegroundColor Green
