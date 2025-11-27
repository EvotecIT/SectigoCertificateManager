# Demonstrates creating an organization.
Connect-Sectigo -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -ApiVersion V25_6 | Out-Null
New-SectigoOrganization -Name "My Org"
