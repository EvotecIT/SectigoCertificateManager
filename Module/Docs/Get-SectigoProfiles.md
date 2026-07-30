---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Get-SectigoProfiles
## SYNOPSIS
Retrieves profiles.

## SYNTAX
### __AllParameterSets
```powershell
Get-SectigoProfiles [-ApiVersion <ApiVersion>] [-CancellationToken <CancellationToken>] [<CommonParameters>]
```

## DESCRIPTION
Lists all profiles for the account using the active Sectigo connection.

## EXAMPLES

### EXAMPLE 1
```powershell
PS> Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "example"; Get-SectigoProfiles
```

Outputs every profile available to the connected account.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `SectigoCertificateManager.Models.Profile`

## RELATED LINKS

- [https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet](https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet)
- [https://github.com/EvotecIT/SectigoCertificateManager](https://github.com/EvotecIT/SectigoCertificateManager)

## NOTES

### Network

Retrieves profile information from the Sectigo API.
