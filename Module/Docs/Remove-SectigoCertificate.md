---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Remove-SectigoCertificate
## SYNOPSIS
Deletes (or revokes) a certificate.

## SYNTAX
### __AllParameterSets
```powershell
Remove-SectigoCertificate [-CertificateId] <int> [-ApiVersion <ApiVersion>] [-ReasonCode <RevocationReason>] [-Reason <string>] [-CancellationToken <CancellationToken>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Uses the active Sectigo connection to remove a certificate, revoking it when using the Admin API.

## EXAMPLES

### EXAMPLE 1
```powershell
PS> Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; Remove-SectigoCertificate -CertificateId 10
```

Permanently removes certificate 10 for the connected account.

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
The identifier of the certificate to delete.

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

### -Reason
Optional human-readable revocation reason text used when revoking via the Admin API.

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

### -ReasonCode
The revocation reason code used when revoking via the Admin API.

```yaml
Type: RevocationReason
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Unspecified, KeyCompromise, CaCompromise, AffiliationChanged, Superseded, CessationOfOperation, CertificateHold, RemoveFromCrl, PrivilegeWithdrawn, AaCompromise

Required: False
Position: named
Default value: [SectigoCertificateManager.RevocationReason]::Unspecified
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

- [https://learn.microsoft.com/powershell/scripting/developer/cmdlet/shouldprocess-attribute](https://learn.microsoft.com/powershell/scripting/developer/cmdlet/shouldprocess-attribute)
- [https://github.com/EvotecIT/SectigoCertificateManager](https://github.com/EvotecIT/SectigoCertificateManager)

## NOTES

### Irreversible

Deleting a certificate cannot be undone.
