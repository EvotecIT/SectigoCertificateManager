---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# New-SectigoCsr
## SYNOPSIS
Generates a certificate signing request and returns the CSR and key material.

## SYNTAX
### __AllParameterSets
```powershell
New-SectigoCsr [-CommonName] <string> [-DnsName <string[]>] [-Organization <string>] [-OrganizationalUnit <string>] [-Locality <string>] [-StateOrProvince <string>] [-Country <string>] [-EmailAddress <string>] [-KeyType <CsrKeyType>] [-KeySize <int>] [-Curve <CsrCurve>] [-HashAlgorithm <string>] [<CommonParameters>]
```

## DESCRIPTION
Generates a certificate signing request and returns the CSR and key material.

## EXAMPLES

### EXAMPLE 1
```powershell
New-SectigoCsr -CommonName 'Name'
```


## PARAMETERS

### -CommonName
Common name (CN) for the subject.

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: True
Position: 0
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -Country
Country code (C) field.

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

### -Curve
Elliptic curve (applies when KeyType is Ecdsa).

```yaml
Type: CsrCurve
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: P256, P384, P521

Required: False
Position: named
Default value: [SectigoCertificateManager.CsrCurve]::P256
Accept pipeline input: False
Accept wildcard characters: False
```

### -DnsName
Optional DNS names for Subject Alternative Name.

```yaml
Type: String[]
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: None
Accept pipeline input: False
Accept wildcard characters: False
```

### -EmailAddress
Email address (E) field.

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

### -HashAlgorithm
Hash algorithm name (e.g., SHA256).

```yaml
Type: String
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: 'SHA256'
Accept pipeline input: False
Accept wildcard characters: False
```

### -KeySize
RSA key size (applies when KeyType is RSA).

```yaml
Type: Int32
Parameter Sets: __AllParameterSets
Aliases: None
Possible values:

Required: False
Position: named
Default value: 2048
Accept pipeline input: False
Accept wildcard characters: False
```

### -KeyType
Key algorithm to use.

```yaml
Type: CsrKeyType
Parameter Sets: __AllParameterSets
Aliases: None
Possible values: Rsa, Ecdsa

Required: False
Position: named
Default value: [SectigoCertificateManager.CsrKeyType]::Rsa
Accept pipeline input: False
Accept wildcard characters: False
```

### -Locality
Locality / City (L) field.

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

### -Organization
Organization (O) field.

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

### -OrganizationalUnit
Organizational Unit (OU) field.

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

### -StateOrProvince
State or Province (ST) field.

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

- `SectigoCertificateManager.Responses.GeneratedCsr`

## RELATED LINKS

- None
