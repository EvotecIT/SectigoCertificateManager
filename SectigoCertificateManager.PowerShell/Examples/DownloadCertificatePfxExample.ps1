# Demonstrates downloading a certificate as PFX.
Get-SectigoCertificateFile -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -CertificateId 10 -Path ./cert.pfx -Format pfx -PfxPassword secret
