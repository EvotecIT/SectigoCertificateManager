Describe 'Get-SectigoCertificateKeystoreLink validation' -Tag 'Cmdlet' {
    BeforeEach {
        try {
            Disconnect-Sectigo -ErrorAction SilentlyContinue
        } catch {
        }
    }

    It 'Throws on invalid CertificateId' {
        { Get-SectigoCertificateKeystoreLink -CertificateId 0 -FormatType 'p12' } |
            Should -Throw -ErrorId 'InvalidCertificateId,SectigoCertificateManager.PowerShell.GetSectigoCertificateKeystoreLinkCommand'
    }

    It 'Throws when Admin connection is missing' {
        { Get-SectigoCertificateKeystoreLink -CertificateId 1 -FormatType 'p12' } |
            Should -Throw -ErrorId 'AdminConnectionRequired,SectigoCertificateManager.PowerShell.GetSectigoCertificateKeystoreLinkCommand'
    }
}

