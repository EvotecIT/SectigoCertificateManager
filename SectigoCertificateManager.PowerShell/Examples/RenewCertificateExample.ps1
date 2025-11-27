# Demonstrates renewing a certificate using an order number.
Connect-Sectigo -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -ApiVersion V25_6 | Out-Null
Invoke-SectigoCertificateRenewal -OrderNumber 10 -Csr "CSR" -DcvMode "Email"
