---
external help file: SectigoCertificateManager-help.xml
Module Name: SectigoCertificateManager
online version: https://github.com/EvotecIT/SectigoCertificateManager
schema: 2.0.0
---
# Disconnect-Sectigo
## SYNOPSIS
Clears shared defaults set by Connect-Sectigo.

## SYNTAX
### __AllParameterSets
```powershell
Disconnect-Sectigo [<CommonParameters>]
```

## DESCRIPTION
Removes Sectigo entries from PSDefaultParameterValues so subsequent cmdlets no longer inherit the connection parameters.

## EXAMPLES

### EXAMPLE 1
```powershell
PS> Disconnect-Sectigo
```

After running, you must provide connection parameters again or call Connect-Sectigo.

## PARAMETERS

### CommonParameters
This cmdlet supports the common parameters: -Debug, -ErrorAction, -ErrorVariable, -InformationAction, -InformationVariable, -OutVariable, -OutBuffer, -PipelineVariable, -Verbose, -WarningAction, and -WarningVariable. For more information, see [about_CommonParameters](http://go.microsoft.com/fwlink/?LinkID=113216).

## INPUTS

- `None`

## OUTPUTS

- `System.Object`

## RELATED LINKS

- None
