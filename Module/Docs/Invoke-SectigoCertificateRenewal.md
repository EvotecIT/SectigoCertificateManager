---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Invoke-SectigoCertificateRenewal
## SYNOPSIS
Renews a certificate (legacy by order number, Admin by certificate id).

## SYNTAX
### Legacy (Default)
```powershell
Invoke-SectigoCertificateRenewal [-OrderNumber] <long> -Csr <string> -DcvMode <DcvMode> [-ApiVersion <ApiVersion>] [-DcvEmail <string>] [-CancellationToken <CancellationToken>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

### AdminById
```powershell
Invoke-SectigoCertificateRenewal [-CertificateId] <int> -Csr <string> -DcvMode <DcvMode> [-DcvEmail <string>] [-CancellationToken <CancellationToken>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Legacy mode: uses order number with the legacy API.
Admin mode: uses certificate id with the Admin Operations API.

## EXAMPLES

### EXAMPLE 1
```powershell
Invoke-SectigoCertificateRenewal -Csr 'Value' -DcvMode 'Value'
```


## PARAMETERS

### -ApiVersion
The API version to use (legacy only).

```yaml
Type: ApiVersion
Parameter Sets: Legacy
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
Parameter Sets: Legacy, AdminById
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CertificateId
Certificate identifier (Admin API only).

```yaml
Type: Int32
Parameter Sets: AdminById
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
Parameter Sets: Legacy, AdminById
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DcvEmail
The domain control validation email address (required when DcvMode is Email).

```yaml
Type: String
Parameter Sets: Legacy, AdminById
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
Parameter Sets: Legacy, AdminById
Aliases: None
Possible values: None, Email, Cname, Http, Https, Txt

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OrderNumber
The legacy order number.

```yaml
Type: Int64
Parameter Sets: Legacy
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

- `System.Int32`

## RELATED LINKS

- [https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet](https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet)
- [https://github.com/EvotecIT/SectigoCertificateManager](https://github.com/EvotecIT/SectigoCertificateManager)

## NOTES

### Network

Contacts the Sectigo API and issues a renewed certificate.
