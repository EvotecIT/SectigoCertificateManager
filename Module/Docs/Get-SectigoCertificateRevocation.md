---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Get-SectigoCertificateRevocation
## SYNOPSIS
Retrieves certificate revocation information.

## SYNTAX
### __AllParameterSets
```powershell
Get-SectigoCertificateRevocation [-CertificateId] <int> [-ApiVersion <ApiVersion>] [-CancellationToken <CancellationToken>] [<CommonParameters>]
```

## DESCRIPTION
Resolves revocation details for the specified certificate using the active Sectigo connection.

## EXAMPLES

### EXAMPLE 1
```powershell
Get-SectigoCertificateRevocation -CertificateId 12345
```

Returns revocation information for the certificate through the active connection.

## PARAMETERS

### -ApiVersion
The API version to use.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `SectigoCertificateManager.Models.CertificateRevocation`

## RELATED LINKS

- [https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet](https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet)
- [https://github.com/EvotecIT/SectigoCertificateManager](https://github.com/EvotecIT/SectigoCertificateManager)

## NOTES

### Network

Requests revocation data from the Sectigo API.
