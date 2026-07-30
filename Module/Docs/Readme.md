---
Module Name: SectigoCertificateManager
Module Guid: 8220d497-40ef-40f5-b1f2-30822973d652
Download Help Link: https://github.com/EvotecIT/SectigoCertificateManager
Help Version: 0.1.0
Locale: en-US
---
# SectigoCertificateManager Module
## Description
SectigoCertificateManager is a PowerShell module to manage Sectigo (formerly Comodo) SSL/TLS certificates via Sectigo APIs.

## SectigoCertificateManager Cmdlets
### [Connect-Sectigo](Connect-Sectigo.md)
Creates shared defaults for Sectigo cmdlets.

### [Disconnect-Sectigo](Disconnect-Sectigo.md)
Clears shared defaults set by Connect-Sectigo.

### [Export-SectigoCertificate](Export-SectigoCertificate.md)
Downloads and exports a certificate.

### [Get-SectigoCertificate](Get-SectigoCertificate.md)
Retrieves certificate details.

### [Get-SectigoCertificateKeystoreLink](Get-SectigoCertificateKeystoreLink.md)
Retrieves a keystore download link for a certificate.

### [Get-SectigoCertificateRevocation](Get-SectigoCertificateRevocation.md)
Retrieves certificate revocation information.

### [Get-SectigoCertificateStatus](Get-SectigoCertificateStatus.md)
Retrieves certificate status.

### [Get-SectigoCertificateTypes](Get-SectigoCertificateTypes.md)
Retrieves available certificate types.

### [Get-SectigoEnrollCertificates](Get-SectigoEnrollCertificates.md)
Lists certificates using the Enroll/Enterprise endpoint (/api/v1/certificates).

### [Get-SectigoInventory](Get-SectigoInventory.md)
Retrieves certificate inventory as exposed by the Sectigo SSL API.

### [Get-SectigoOrderHistory](Get-SectigoOrderHistory.md)
Retrieves order history.

### [Get-SectigoOrders](Get-SectigoOrders.md)
Retrieves certificate orders.

### [Get-SectigoOrdersPage](Get-SectigoOrdersPage.md)
Retrieves a single page of orders.

### [Get-SectigoOrganizations](Get-SectigoOrganizations.md)
Retrieves organizations.

### [Get-SectigoProfile](Get-SectigoProfile.md)
Retrieves a profile.

### [Get-SectigoProfiles](Get-SectigoProfiles.md)
Retrieves profiles.

### [Invoke-SectigoCertificateRenewal](Invoke-SectigoCertificateRenewal.md)
Renews a certificate (legacy by order number, Admin by certificate id).

### [New-SectigoCsr](New-SectigoCsr.md)
Generates a certificate signing request and returns the CSR and key material.

### [New-SectigoOrder](New-SectigoOrder.md)
Creates a new certificate order.

### [New-SectigoOrganization](New-SectigoOrganization.md)
Creates a new organization.

### [Remove-SectigoCertificate](Remove-SectigoCertificate.md)
Deletes (or revokes) a certificate.

### [Stop-SectigoOrder](Stop-SectigoOrder.md)
Cancels an order.

### [Update-SectigoCertificate](Update-SectigoCertificate.md)
Renews an existing certificate.

### [Wait-SectigoOrder](Wait-SectigoOrder.md)
Waits for an order to reach a terminal status.
