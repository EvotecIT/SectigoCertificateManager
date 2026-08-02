---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Get-SectigoOrganizations
## SYNOPSIS
Retrieves organizations.

## SYNTAX
### __AllParameterSets
```powershell
Get-SectigoOrganizations [-ApiVersion <ApiVersion>] [-CancellationToken <CancellationToken>] [<CommonParameters>]
```

## DESCRIPTION
Builds an API client and lists all organizations for the account.

## EXAMPLES

### EXAMPLE 1
```powershell
$credential = Get-Credential; Connect-Sectigo -BaseUrl 'https://cert-manager.com/api' -Username $credential.UserName -Password $credential.GetNetworkCredential().Password -CustomerUri 'tenant'; Get-SectigoOrganizations
```

Creates a legacy API connection and lists its organizations.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `SectigoCertificateManager.Models.Organization`

## RELATED LINKS

- [https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet](https://learn.microsoft.com/powershell/scripting/developer/cmdlet/writing-a-cmdlet)
- [https://github.com/EvotecIT/SectigoCertificateManager](https://github.com/EvotecIT/SectigoCertificateManager)

## NOTES

### Legacy connection required

This cmdlet does not support an Admin (OAuth2) connection. Connect with legacy credentials before calling it.

### Network

Requests organization data from the Sectigo API.
