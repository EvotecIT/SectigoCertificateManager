---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Get-SectigoCertificate
## SYNOPSIS
Retrieves certificate details.

## SYNTAX
### List (Default)
```powershell
Get-SectigoCertificate [-ApiVersion <ApiVersion>] [-Size <int>] [-Position <int>] [-Status <CertificateStatus>] [-OrgId <int>] [-Requester <string>] [-ExpiresWithinDays <int>] [-MaxCertificatesToScan <int>] [-ExpiresBefore <datetime>] [-ExpiresAfter <datetime>] [-Detailed] [-CancellationToken <CancellationToken>] [<CommonParameters>]
```

### ById
```powershell
Get-SectigoCertificate [-CertificateId] <int> [-ApiVersion <ApiVersion>] [-CancellationToken <CancellationToken>] [<CommonParameters>]
```

## DESCRIPTION
Uses CertificateService to retrieve certificate information
for the active Sectigo connection (legacy SCM or Admin Operations API).

## EXAMPLES

### EXAMPLE 1
```powershell
PS> Connect-Sectigo -ClientId "<client id>" -ClientSecret "<client secret>"; Get-SectigoCertificate -CertificateId 12345
```

Connects using the Admin API and retrieves certificate 12345.

### EXAMPLE 2
```powershell
PS> Connect-Sectigo -BaseUrl "https://cert-manager.com/api" -Username "user" -Password "pass" -CustomerUri "tenant"; Get-SectigoCertificate -Size 30
```

Connects using legacy credentials and lists the latest 30 certificates.

### EXAMPLE 3
```powershell
PS> Connect-Sectigo -ClientId "<client id>" -ClientSecret "<client secret>"; Get-SectigoCertificate -Size 50 -Status Issued -Requester "user@example.com" -ExpiresBefore (Get-Date).AddDays(30) -Detailed
```

Connects using the Admin API and lists detailed certificates that are issued, requested by the specified user and expiring within the next 30 days.

## PARAMETERS

### -ApiVersion
The API version to use when using the legacy API.

```yaml
Type: ApiVersion
Parameter Sets: List, ById
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
Parameter Sets: List, ById
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -CertificateId
The certificate identifier.

```yaml
Type: Int32
Parameter Sets: ById
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Detailed
When specified, retrieves full certificate details for each entry when using the Admin Operations API.
Ignored when using the legacy API, which already returns detailed certificate objects.

```yaml
Type: SwitchParameter
Parameter Sets: List
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExpiresAfter
Optional lower bound for the certificate expiration date. When specified, only certificates
expiring on or after this date (inclusive) are returned when using the Admin API.
Ignored for the legacy API.

```yaml
Type: Nullable`1
Parameter Sets: List
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExpiresBefore
Optional upper bound for the certificate expiration date. When specified, only certificates
expiring on or before this date (inclusive) are returned when using the Admin API.
Ignored for the legacy API.

```yaml
Type: Nullable`1
Parameter Sets: List
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -ExpiresWithinDays
Optional filter for certificates that expire within the specified number of days from now.
When specified, this filter is only applied for Admin (OAuth2) connections and uses detailed
certificate information. Ignored for legacy API connections, which do not expose an Admin-style
expiry filter.

```yaml
Type: Nullable`1
Parameter Sets: List
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -MaxCertificatesToScan
Optional maximum number of certificates to scan when searching for expiring certificates.
Intended primarily for testing and exploratory use when combined with ExpiresWithinDays.

```yaml
Type: Nullable`1
Parameter Sets: List
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -OrgId
Optional organization identifier filter. When specified, applies only to Admin connections.

```yaml
Type: Int32
Parameter Sets: List
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
Type: Int32
Parameter Sets: List
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Requester
Optional requester filter. When specified, applies only to Admin connections.

```yaml
Type: String
Parameter Sets: List
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Size
Maximum number of certificates to retrieve.

```yaml
Type: Int32
Parameter Sets: List
Aliases: None
Possible values:

Required: False
Position: named
Default value: 30
Accept pipeline input: False
Accept wildcard characters: False
```

### -Status
Optional certificate status filter (for example, Issued, Expired). When specified, applies only to Admin connections.

```yaml
Type: CertificateStatus
Parameter Sets: List
Aliases: None
Possible values: Any, Requested, Issued, Approved, Applied, Declined, Downloaded, Rejected, AwaitingApproval, Invalid, Replaced, Unmanaged, SAApproved, Init, Revoked, Expired, EnrolledPendingDownload, NotEnrolled, External

Required: False
Position: named
Default value: [SectigoCertificateManager.CertificateStatus]::Any
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

### Network

Queries the Sectigo API to fetch certificate data.
