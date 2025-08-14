Describe 'Renew-SectigoCertificate validation' -Tag 'Cmdlet' {
    It 'Throws on invalid OrderNumber' {
        $params = @{ BaseUrl='https://example.com'; Username='user'; Password='pass'; CustomerUri='cst'; OrderNumber=0; Csr='csr'; DcvMode='Email' }
        { Renew-SectigoCertificate @params } | Should -Throw -ErrorId 'InvalidOrderNumber,SectigoCertificateManager.PowerShell.RenewSectigoCertificateCommand'
    }
}
