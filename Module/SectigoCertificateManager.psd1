@{
    AliasesToExport      = @()
    Author               = 'Przemyslaw Klys'
    CmdletsToExport      = @('Connect-Sectigo', 'Disconnect-Sectigo', 'Export-SectigoCertificate', 'Get-SectigoCertificate', 'Get-SectigoCertificateRevocation', 'Get-SectigoCertificateStatus', 'Get-SectigoCertificateTypes', 'Get-SectigoCertificates', 'Get-SectigoInventory', 'Get-SectigoEnrollCertificates', 'Get-SectigoOrderHistory', 'Get-SectigoOrders', 'Get-SectigoOrdersPage', 'Get-SectigoOrganizations', 'Get-SectigoProfile', 'Get-SectigoProfiles', 'Get-SectigoAdminCertificates', 'New-SectigoOrder', 'New-SectigoOrganization', 'Remove-SectigoCertificate', 'Renew-SectigoCertificate', 'Stop-SectigoOrder', 'Update-SectigoCertificate', 'Wait-SectigoOrder')
    CompanyName          = 'Evotec'
    CompatiblePSEditions = @('Desktop', 'Core')
    Copyright            = '(c) 2011 - 2025 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'SectigoCertificateManager'
    FunctionsToExport    = @()
    GUID                 = '8220d497-40ef-40f5-b1f2-30822973d652'
    ModuleVersion        = '0.1.0'
    PowerShellVersion    = '5.1'
    PrivateData          = @{
        PSData = @{
            ProjectUri               = 'https://github.com/EvotecIT/SectigoCertificateManager'
            RequireLicenseAcceptance = $false
            Tags                     = @('Windows', 'MacOS', 'Linux')
        }
    }
    RootModule           = 'SectigoCertificateManager.psm1'
}
