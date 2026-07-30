---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Connect-Sectigo
## SYNOPSIS
Creates shared defaults for Sectigo cmdlets.

## SYNTAX
### Legacy
```powershell
Connect-Sectigo -Username <string> -Password <string> -CustomerUri <string> [-BaseUrl <string>] [-ApiVersion <ApiVersion>] [<CommonParameters>]
```

### Admin
```powershell
Connect-Sectigo -ClientId <string> -ClientSecret <string> [-Instance <string>] [-AdminBaseUrl <string>] [-TokenUrl <string>] [<CommonParameters>]
```

## DESCRIPTION
Stores connection parameters for either the legacy SCM API (username/password)
or the Admin Operations API (OAuth2 client credentials). Other Sectigo cmdlets
reuse the active connection without repeating authentication arguments.

## EXAMPLES

### EXAMPLE 1
```powershell
PS> Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "tenant" -ApiVersion V25_6
```

Subsequent Get-SectigoOrders or Get-SectigoCertificate calls will use the legacy API configuration.

### EXAMPLE 2
```powershell
PS> Connect-Sectigo -ClientId "<client id>" -ClientSecret "<client secret>" -Instance "enterprise" -AdminBaseUrl "https://admin.enterprise.sectigo.com"
```

Subsequent certificate cmdlets such as Get-SectigoCertificate and Export-SectigoCertificate will route through the Admin API.

## PARAMETERS

### -AdminBaseUrl
Base URL of the Admin API.

```yaml
Type: String
Parameter Sets: Admin
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ApiVersion
The API version to use.

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

### -BaseUrl
The API base URL (e.g., https://cert-manager.com/ssl).

```yaml
Type: String
Parameter Sets: Legacy
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientId
The OAuth2 client identifier for the Admin API.

```yaml
Type: String
Parameter Sets: Admin
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ClientSecret
The OAuth2 client secret for the Admin API.

```yaml
Type: String
Parameter Sets: Admin
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CustomerUri
The customer URI assigned by Sectigo.

```yaml
Type: String
Parameter Sets: Legacy
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Instance
The Sectigo instance name (for example, enterprise).

```yaml
Type: String
Parameter Sets: Admin
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Password
The password for authentication.

```yaml
Type: String
Parameter Sets: Legacy
Aliases: None
Possible values:

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -TokenUrl
OAuth2 token endpoint URL for the Admin API.

```yaml
Type: String
Parameter Sets: Admin
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Username
The user name for authentication.

```yaml
Type: String
Parameter Sets: Legacy
Aliases: None
Possible values:

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

- `System.Object`

## RELATED LINKS

- None
