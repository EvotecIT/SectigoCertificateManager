Describe 'Remove-SectigoCertificate revocation reason parameters' -Tag 'Cmdlet' {
    It 'Accepts ReasonCode and Reason without error' {
        $params = @{
            CertificateId = 1
            ReasonCode    = 'KeyCompromise'
            Reason        = 'Key compromised'
        }

        # We only verify that parameter binding succeeds and the cmdlet can be invoked.
        # Behaviour against the actual API is covered by C# tests.
        { Remove-SectigoCertificate @params -WhatIf } | Should -Not -Throw
    }
}

