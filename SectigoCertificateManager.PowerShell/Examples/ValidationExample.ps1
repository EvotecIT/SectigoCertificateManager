# Demonstrates validation behavior in the PowerShell module
try {
    New-SectigoOrder -BaseUrl 'https://example.com/' -Username 'user' -Password 'pass' -CustomerUri 'cust' -CommonName '' -ProfileId 1 -Term 0
} catch {
    Write-Host $_.Exception.Message
}
