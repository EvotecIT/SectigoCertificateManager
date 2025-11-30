Import-Module "$PSScriptRoot\..\SectigoCertificateManager.psd1" -Force

# Admin connection
Connect-Sectigo -ClientId "client-id" `
                -ClientSecret "client-secret" `
                -Instance "enterprise" `
                -AdminBaseUrl "https://admin.enterprise.sectigo.com" | Out-Null

# Generate CSR (includes SANs and key material)
$csr = New-SectigoCsr -CommonName "ilm.eurofins-diatherix.com" `
                      -DnsName "ilm.eurofins-diatherix.com","www.ilm.eurofins-diatherix.com" `
                      -Organization "Eurofins" `
                      -OrganizationalUnit "IT" `
                      -Country "US"

# Renew by certificate id using generated CSR
$newId = Invoke-SectigoCertificateRenewal -CertificateId 11552108 `
    -Csr $csr.Csr `
    -DcvMode Email `
    -DcvEmail "admin@example.com" `
    -Verbose

# Download renewed cert (PEM and PFX)
Export-SectigoCertificate -CertificateId $newId -Path "./renewed.pem" -Format Pem
Export-SectigoCertificate -CertificateId $newId -Path "./renewed.pfx" -Format Pfx -PfxPassword (Read-Host -AsSecureString "Pfx password")
