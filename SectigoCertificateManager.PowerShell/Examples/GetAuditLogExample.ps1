# Demonstrates retrieving audit log entries.
Get-SectigoAuditLog -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -From (Get-Date).AddDays(-7) -To (Get-Date) -ApiVersion V25_6
