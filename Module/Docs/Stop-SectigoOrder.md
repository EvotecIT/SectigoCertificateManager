---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Stop-SectigoOrder
## SYNOPSIS
Cancels an order.

## SYNTAX
### __AllParameterSets
```powershell
Stop-SectigoOrder [-OrderId] <int> [-ApiVersion <ApiVersion>] [-CancellationToken <CancellationToken>] [-WhatIf] [-Confirm] [<CommonParameters>]
```

## DESCRIPTION
Calls the cancel endpoint using the active Sectigo connection.

## EXAMPLES

### EXAMPLE 1
```powershell
PS> Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; Stop-SectigoOrder -OrderId 100
```

Cancels the specified order immediately for the connected account.

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

### -OrderId
The identifier of the order to cancel.

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

- `System.Object`

## RELATED LINKS

- [https://learn.microsoft.com/powershell/scripting/developer/cmdlet/confirmimpact-attribute](https://learn.microsoft.com/powershell/scripting/developer/cmdlet/confirmimpact-attribute)
- [https://github.com/EvotecIT/SectigoCertificateManager](https://github.com/EvotecIT/SectigoCertificateManager)

## NOTES

### Irreversible

Once an order is cancelled the action cannot be reversed.
