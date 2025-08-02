# Demonstrates renewing a certificate for an order.
New-SectigoRenewal -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -OrderId 10 -Csr "<csr>" -DcvMode "EMAIL" -DcvEmail "admin@example.com" -ApiVersion V25_6

