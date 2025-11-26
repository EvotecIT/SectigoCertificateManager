Describe 'Wait-SectigoOrder validation' -Tag 'Cmdlet' {
    It 'Throws on invalid OrderId' {
        $params = @{ OrderId=0 }
        { Wait-SectigoOrder @params } | Should -Throw -ErrorId 'InvalidOrderId,SectigoCertificateManager.PowerShell.WaitSectigoOrderCommand'
    }
}
