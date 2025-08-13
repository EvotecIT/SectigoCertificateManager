# Demonstrates exporting a certificate.
Export-SectigoCertificate -BaseUrl "https://example.com" -Username "user" -Password "pass" -CustomerUri "cst1" -CertificateId 10 -Path "cert.pem" -Format Pem

