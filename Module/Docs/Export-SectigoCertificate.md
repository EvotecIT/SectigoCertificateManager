---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Export-SectigoCertificate
## SYNOPSIS
Downloads and exports a certificate.

## SYNTAX
### __AllParameterSets
```powershell
Export-SectigoCertificate [-CertificateId] <int> -Path <string> [-ApiVersion <ApiVersion>] [-Format <CertificateFileFormat>] [-PfxPassword <string>] [-CancellationToken <CancellationToken>] [<CommonParameters>]
```

## DESCRIPTION
Uses the active Sectigo connection, downloads the certificate, and saves it to disk.

## EXAMPLES

### EXAMPLE 1
```powershell
Export-SectigoCertificate -CertificateId 12345 -Path '.\certificate.pem' -Format Pem
```

Downloads the certificate and writes it to the requested path in PEM format.

## PARAMETERS

### -ApiVersion
The API version to use when calling the legacy API.

```yaml
Type: ApiVersion
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: V25_4, V25_5, V25_6

Required: False
Position: named
Default value: [SectigoCertificateManager.ApiVersion]::V25_6
Accept pipeline input: False
Accept wildcard characters: False
```

### -CancellationToken
Optional cancellation token.

```yaml
Type: CancellationToken
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CertificateId
The certificate identifier.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Format
Export format.

```yaml
Type: CertificateFileFormat
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Pem, Der, Pfx

Required: False
Position: named
Default value: [SectigoCertificateManager.PowerShell.CertificateFileFormat]::Pem
Accept pipeline input: False
Accept wildcard characters: False
```

### -Path
Destination file path.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -PfxPassword
Password protecting the PFX when Pfx is used.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `None`

## RELATED LINKS

- [https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet](https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet)
- [https://github.com/EvotecIT/SectigoCertificateManager](https://github.com/EvotecIT/SectigoCertificateManager)

## NOTES

### Network

Requires connectivity to the Sectigo API.
