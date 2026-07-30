---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Update-SectigoCertificate
## SYNOPSIS
Renews an existing certificate.

## SYNTAX
### __AllParameterSets
```powershell
Update-SectigoCertificate [-CertificateId] <int> -Csr <string> -DcvMode <DcvMode> [-ApiVersion <ApiVersion>] [-DcvEmail <string>] [-CancellationToken <CancellationToken>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Uses the active Sectigo connection and submits a RenewCertificateRequest to the appropriate renew endpoint.

## EXAMPLES

### EXAMPLE 1
```powershell
Update-SectigoCertificate -Csr 'Value' -DcvMode 'Value'
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

### -CertificateId
The identifier of the certificate to renew.

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

### -Csr
The certificate signing request.

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

### -DcvEmail
The domain control validation email address.

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

### -DcvMode
The domain control validation mode.

```yaml
Type: DcvMode
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: None, Email, Cname, Http, Https, Txt

Required: True
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

- `System.Int32`

## RELATED LINKS

- [https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet](https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet)
- [https://github.com/EvotecIT/SectigoCertificateManager](https://github.com/EvotecIT/SectigoCertificateManager)

## NOTES

### Network

Contacts the Sectigo API and replaces the specified certificate.
