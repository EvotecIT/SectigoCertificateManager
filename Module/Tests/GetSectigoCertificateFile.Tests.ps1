Describe 'Get-SectigoCertificateFile validation' -Tag 'Cmdlet' {
    It 'Throws on invalid CertificateId' {
        $params = @{ BaseUrl='https://example.com'; Username='u'; Password='p'; CustomerUri='c'; CertificateId=0; Path='out.pfx' }
        { Get-SectigoCertificateFile @params } | Should -Throw
    }

    It 'Throws when Path is null or whitespace' {
        $params = @{ BaseUrl='https://example.com'; Username='u'; Password='p'; CustomerUri='c'; CertificateId=1; Path=' ' }
        { Get-SectigoCertificateFile @params } | Should -Throw
    }
}
