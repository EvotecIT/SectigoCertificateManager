---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# New-SectigoOrder
## SYNOPSIS
Creates a new certificate order.

## SYNTAX
### __AllParameterSets
```powershell
New-SectigoOrder -CommonName <string> -ProfileId <int> [-ApiVersion <ApiVersion>] [-Term <int>] [-SubjectAlternativeNames <string[]>] [-CancellationToken <CancellationToken>] [<CommonParameters>]
```

## DESCRIPTION
Builds an API client and submits an IssueCertificateRequest.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SectigoOrder -CommonName 'Name' -ProfileId 1
```


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
Default value: None
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

### -CommonName
The certificate common name.

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

### -ProfileId
The profile identifier used for issuance.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -SubjectAlternativeNames
Optional subject alternative names.

```yaml
Type: String[]
Parameter Sets: __AllParameterSets
Aliases: SubjectAlternativeName, San
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Term
The certificate term in months.

```yaml
Type: Int32
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

- `SectigoCertificateManager.Models.Certificate`

## RELATED LINKS

- [https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet](https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet)
- [https://github.com/EvotecIT/SectigoCertificateManager](https://github.com/EvotecIT/SectigoCertificateManager)

## NOTES

### Charges

Issuing a certificate may incur costs on your Sectigo account.
