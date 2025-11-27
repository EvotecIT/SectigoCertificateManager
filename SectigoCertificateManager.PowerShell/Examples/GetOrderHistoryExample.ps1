# Demonstrates retrieving history for an order.
Connect-Sectigo -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -ApiVersion V25_6 | Out-Null
Get-SectigoOrderHistory -OrderId 10
