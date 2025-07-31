Describe 'Wait-SectigoOrder validation' -Tag 'Cmdlet' {
    It 'Throws on invalid OrderId' {
        $params = @{ BaseUrl='https://example.com'; Username='u'; Password='p'; CustomerUri='c'; OrderId=0 }
        { Wait-SectigoOrder @params } | Should -Throw -ErrorId 'InvalidOrderId,SectigoCertificateManager.PowerShell.WaitSectigoOrderCommand'
    }
}
