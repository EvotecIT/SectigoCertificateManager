---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Get-SectigoInventory
## SYNOPSIS
Retrieves certificate inventory as exposed by the Sectigo SSL API.

## SYNTAX
### __AllParameterSets
```powershell
Get-SectigoInventory [-ApiVersion <ApiVersion>] [-Size <int>] [-Position <int>] [-DateFrom <datetime>] [-DateTo <datetime>] [-CancellationToken <CancellationToken>] [<CommonParameters>]
```

## DESCRIPTION
Wraps the inventory CSV endpoint and returns parsed InventoryRecord objects.

## EXAMPLES

### EXAMPLE 1
```powershell
PS> $to = (Get-Date).Date.AddDays(30); Get-SectigoInventory -DateTo $to
```

Filters inventory by an upper expiration bound.

## PARAMETERS

### -ApiVersion
API version to use.

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

### -DateFrom
Filter certificates issued or updated from this date (yyyy-MM-dd).

```yaml
Type: Nullable`1
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -DateTo
Filter certificates issued or updated up to this date (yyyy-MM-dd).

```yaml
Type: Nullable`1
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Position
Position offset for paging.

```yaml
Type: Nullable`1
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Size
Maximum number of records to return.

```yaml
Type: Nullable`1
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

- `SectigoCertificateManager.Models.InventoryRecord`

## RELATED LINKS

- None
