# Demonstrates renewing a certificate using an order number.
Invoke-SectigoCertificateRenewal -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -OrderNumber 10 -Csr "CSR" -DcvMode "Email"
