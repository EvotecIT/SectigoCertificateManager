Import-Module PSPublishModule -Force

Get-ProjectVersion -Path "C:\Support\GitHub\SectigoCertificateManager" -ExcludeFolders @('C:\Support\GitHub\SectigoCertificateManager\Module\Artefacts') | Format-Table

Set-ProjectVersion -Path "C:\Support\GitHub\SectigoCertificateManager" -NewVersion "0.1.0" -Verbose -ExcludeFolders @('C:\Support\GitHub\SectigoCertificateManager\Module\Artefacts')
