Describe 'New-SectigoOrder parameter validation' -Tag 'Cmdlet' {
    It 'Throws when SubjectAlternativeNames contains null or whitespace' {
        $params = @{ 
            BaseUrl  = 'https://example.com'
            Username = 'user'
            Password = 'pass'
            CustomerUri = 'cust'
            CommonName = 'example.com'
            ProfileId = 1
            SubjectAlternativeNames = @('valid', '')
        }
        { New-SectigoOrder @params } | Should -Throw -ErrorId 'InvalidSubjectAlternativeName,SectigoCertificateManager.PowerShell.NewSectigoOrderCommand'
    }

    It 'Throws when CommonName is null or whitespace' {
        $params = @{
            BaseUrl  = 'https://example.com'
            Username = 'user'
            Password = 'pass'
            CustomerUri = 'cust'
            CommonName = ' '
            ProfileId = 1
        }
        { New-SectigoOrder @params } | Should -Throw -ErrorId 'InvalidCommonName,SectigoCertificateManager.PowerShell.NewSectigoOrderCommand'
    }
}
