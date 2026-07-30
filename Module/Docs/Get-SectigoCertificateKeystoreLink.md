---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Get-SectigoCertificateKeystoreLink
## SYNOPSIS
Retrieves a keystore download link for a certificate.

## SYNTAX
### __AllParameterSets
```powershell
Get-SectigoCertificateKeystoreLink [-CertificateId] <int> -FormatType <KeystoreFormatType> [-Passphrase <string>] [-CancellationToken <CancellationToken>] [<CommonParameters>]
```

## DESCRIPTION
Uses the Admin Operations API via CertificateService to create a
keystore download link for the specified certificate. This cmdlet requires an
Admin (OAuth2) connection created with Connect-Sectigo -ClientId/-ClientSecret.

## EXAMPLES

### EXAMPLE 1
```powershell
PS> Connect-Sectigo -ClientId "<client id>" -ClientSecret "<client secret>"; Get-SectigoCertificateKeystoreLink -CertificateId 12345 -FormatType p12
```

Returns a download link for a PKCS#12 keystore containing the specified certificate.

## PARAMETERS

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
The certificate identifier.

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

### -FormatType
Keystore format type.

```yaml
Type: KeystoreFormatType
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Key, P12, P12Aes, Jks, Pem

Required: True
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Passphrase
Optional passphrase used to protect the keystore.

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

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `System.String`

## RELATED LINKS

- None

## NOTES

### Admin only

This cmdlet is not available when connected to the legacy SCM API.
