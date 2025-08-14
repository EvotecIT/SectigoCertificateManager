@{
    AliasesToExport      = @()
    Author               = 'Przemyslaw Klys'
    CmdletsToExport      = @('Get-SectigoCertificate', 'Get-SectigoOrders', 'Get-SectigoOrdersPage', 'Get-SectigoOrganizations', 'Get-SectigoProfile', 'Get-SectigoProfiles', 'New-SectigoOrder', 'Stop-SectigoOrder', 'Update-SectigoCertificate', 'Renew-SectigoCertificate', 'Remove-SectigoCertificate', 'Wait-SectigoOrder')
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
            ProjectUri = 'https://github.com/EvotecIT/SectigoCertificateManager'
            Tags       = @('Windows', 'MacOS', 'Linux')
        }
    }
    RootModule           = 'SectigoCertificateManager.psm1'
}
