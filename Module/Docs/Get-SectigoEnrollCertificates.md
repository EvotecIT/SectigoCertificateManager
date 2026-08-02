---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Get-SectigoEnrollCertificates
## SYNOPSIS
Lists certificates using the Enroll/Enterprise endpoint (/api/v1/certificates).

## SYNTAX
### __AllParameterSets
```powershell
Get-SectigoEnrollCertificates -BaseUrl <string> -Username <string> -Password <string> -CustomerUri <string> [-Size <int>] [-Position <int>] [-CancellationToken <CancellationToken>] [<CommonParameters>]
```

## DESCRIPTION
Use this when your tenant exposes the Enroll API shown in the portal (e.g., https://yourtenant.enroll.enterprise.sectigo.com/api/v1/certificates).

## EXAMPLES

### EXAMPLE 1
```powershell
$credential = Get-Credential; Get-SectigoEnrollCertificates -BaseUrl 'https://tenant.enroll.enterprise.sectigo.com' -Username $credential.UserName -Password $credential.GetNetworkCredential().Password -CustomerUri 'tenant' -Size 50
```

Prompts for Enroll credentials and returns the first page of certificates for the tenant.

## PARAMETERS

### -BaseUrl
Base URL, e.g., https://company.enroll.enterprise.sectigo.com.

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

### -CustomerUri
Customer URI.

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

### -Password
Password for authentication.

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

### -Position
Paging offset.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: 0
Accept pipeline input: False
Accept wildcard characters: False
```

### -Size
Maximum records to fetch.

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: 30
Accept pipeline input: False
Accept wildcard characters: False
```

### -Username
User name for authentication.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `System.Text.Json.JsonElement`

## RELATED LINKS

- None
