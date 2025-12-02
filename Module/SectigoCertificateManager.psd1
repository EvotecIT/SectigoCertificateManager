@{
    AliasesToExport      = @('Get-SectigoCertificates')
    Author               = 'Przemyslaw Klys'
    CmdletsToExport      = @('Connect-Sectigo', 'Disconnect-Sectigo', 'Export-SectigoCertificate', 'Get-SectigoCertificate', 'Get-SectigoCertificateKeystoreLink', 'Get-SectigoCertificateRevocation', 'Get-SectigoCertificateStatus', 'Get-SectigoCertificateTypes', 'Get-SectigoEnrollCertificates', 'Get-SectigoInventory', 'Get-SectigoOrderHistory', 'Get-SectigoOrders', 'Get-SectigoOrdersPage', 'Get-SectigoOrganizations', 'Get-SectigoProfile', 'Get-SectigoProfiles', 'New-SectigoCsr', 'New-SectigoOrder', 'New-SectigoOrganization', 'Remove-SectigoCertificate', 'Invoke-SectigoCertificateRenewal', 'Stop-SectigoOrder', 'Update-SectigoCertificate', 'Wait-SectigoOrder')
    CompanyName          = 'Evotec'
    CompatiblePSEditions = @('Desktop', 'Core')
    Copyright            = '(c) 2011 - 2025 Przemyslaw Klys @ Evotec. All rights reserved.'
    Description          = 'SectigoCertificateManager is a PowerShell module to manage Sectigo (formerly Comodo) SSL/TLS certificates via Sectigo APIs.'
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