Describe 'Invoke-SectigoCertificateRenewal validation' -Tag 'Cmdlet' {
    It 'Throws on invalid OrderNumber' {
        $params = @{ OrderNumber=0; Csr='csr'; DcvMode='Email' }
        { Invoke-SectigoCertificateRenewal @params } | Should -Throw -ErrorId 'InvalidOrderNumber,SectigoCertificateManager.PowerShell.RenewSectigoCertificateCommand'
    }
}
