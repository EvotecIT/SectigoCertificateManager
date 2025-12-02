#Import-Module "C:\Support\GitHub\PSPublishModule\PSPublishModule.psd1" -Force

Build-Module -ModuleName 'SectigoCertificateManager' {
    # Usual defaults as per standard module
    $Manifest = [ordered] @{
        ModuleVersion        = '0.1.0'
        CompatiblePSEditions = @('Desktop', 'Core')
        GUID                 = '8220d497-40ef-40f5-b1f2-30822973d652'
        Author               = 'Przemyslaw Klys'
        CompanyName          = 'Evotec'
        Copyright            = "(c) 2011 - $((Get-Date).Year) Przemyslaw Klys @ Evotec. All rights reserved."
        Description          = 'SectigoCertificateManager is a PowerShell module to manage Sectigo (formerly Comodo) SSL/TLS certificates via Sectigo APIs.'
        Tags                 = @('Windows', 'MacOS', 'Linux')
        #IconUri              = ''
        ProjectUri           = 'https://github.com/EvotecIT/SectigoCertificateManager'
        PowerShellVersion    = '5.1'
    }
    New-ConfigurationManifest @Manifest


    $ConfigurationFormat = [ordered] @{
        RemoveComments                              = $false

        PlaceOpenBraceEnable                        = $true
        PlaceOpenBraceOnSameLine                    = $true
        PlaceOpenBraceNewLineAfter                  = $true
        PlaceOpenBraceIgnoreOneLineBlock            = $false

        PlaceCloseBraceEnable                       = $true
        PlaceCloseBraceNewLineAfter                 = $false
        PlaceCloseBraceIgnoreOneLineBlock           = $false
        PlaceCloseBraceNoEmptyLineBefore            = $true

        UseConsistentIndentationEnable              = $true
        UseConsistentIndentationKind                = 'space'
        UseConsistentIndentationPipelineIndentation = 'IncreaseIndentationAfterEveryPipeline'
        UseConsistentIndentationIndentationSize     = 4

        UseConsistentWhitespaceEnable               = $true
        UseConsistentWhitespaceCheckInnerBrace      = $true
        UseConsistentWhitespaceCheckOpenBrace       = $true
        UseConsistentWhitespaceCheckOpenParen       = $true
        UseConsistentWhitespaceCheckOperator        = $true
        UseConsistentWhitespaceCheckPipe            = $true
        UseConsistentWhitespaceCheckSeparator       = $true

        AlignAssignmentStatementEnable              = $true
        AlignAssignmentStatementCheckHashtable      = $true

        UseCorrectCasingEnable                      = $true
    }
    # format PSD1 and PSM1 files when merging into a single file
    # enable formatting is not required as Configuration is provided
    New-ConfigurationFormat -ApplyTo 'OnMergePSM1', 'OnMergePSD1' -Sort None @ConfigurationFormat
    # format PSD1 and PSM1 files within the module
    # enable formatting is required to make sure that formatting is applied (with default settings)
    New-ConfigurationFormat -ApplyTo 'DefaultPSD1', 'DefaultPSM1' -EnableFormatting -Sort None
    # when creating PSD1 use special style without comments and with only required parameters
    New-ConfigurationFormat -ApplyTo 'DefaultPSD1', 'OnMergePSD1' -PSD1Style 'Minimal'

    # configuration for documentation, at the same time it enables documentation processing
    New-ConfigurationDocumentation -Enable:$false -StartClean -UpdateWhenNew -PathReadme 'Docs\Readme.md' -Path 'Docs'

    New-ConfigurationImportModule -ImportSelf -ImportRequiredModules

    $newConfigurationBuildSplat = @{
        Enable                            = $true
        SignModule                        = $true
        MergeModuleOnBuild                = $true
        MergeFunctionsFromApprovedModules = $true
        CertificateThumbprint             = '483292C9E317AA13B07BB7A96AE9D1A5ED9E7703'
        NETProjectPath                    = "$PSScriptRoot\..\..\SectigoCertificateManager.PowerShell"
        ResolveBinaryConflicts            = $true
        ResolveBinaryConflictsName        = 'SectigoCertificateManager.PowerShell'
        NETProjectName                    = 'SectigoCertificateManager.PowerShell'
        NETBinaryModule                   = 'SectigoCertificateManager.PowerShell.dll'
        NETConfiguration                  = 'Release'
        NETFramework                      = 'net472', 'net8.0'
        DotSourceLibraries                = $true
        NETSearchClass                    = 'SectigoCertificateManager.PowerShell.NewSectigoOrderCommand'
        RefreshPSD1Only                   = $false
    }

    New-ConfigurationBuild @newConfigurationBuildSplat

    New-ConfigurationArtefact -Type Unpacked -Enable -Path "$PSScriptRoot\..\Artefacts\Unpacked" -RequiredModulesPath "$PSScriptRoot\..\Artefacts\Unpacked\Modules"
    New-ConfigurationArtefact -Type Packed -Enable -Path "$PSScriptRoot\..\Artefacts\Packed" -IncludeTagName -ArtefactName "SectigoCertificateManager-PowerShellModule.<TagModuleVersionWithPreRelease>.zip" -ID 'ToGitHub'

    # global options for publishing to github/psgallery
    #New-ConfigurationPublish -Type PowerShellGallery -FilePath 'C:\Support\Important\PowerShellGalleryAPI.txt' -Enabled:$true
    #New-ConfigurationPublish -Type GitHub -FilePath 'C:\Support\Important\GitHubAPI.txt' -UserName 'EvotecIT' -Enabled:$true -ID 'ToGitHub' -OverwriteTagName 'SectigoCertificateManager-PowerShellModule.<TagModuleVersionWithPreRelease>'
}
