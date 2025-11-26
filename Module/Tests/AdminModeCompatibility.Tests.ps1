Describe 'Admin-mode compatibility' -Tag 'Cmdlet' {
    BeforeEach {
        try {
            Disconnect-Sectigo -ErrorAction SilentlyContinue
        } catch {
        }
    }

    It 'New-SectigoOrder throws in Admin mode' {
        Connect-Sectigo -ClientId 'id' -ClientSecret 'secret' | Out-Null

        { New-SectigoOrder -CommonName 'example.com' -ProfileId 1 } |
            Should -Throw -ErrorId 'InvalidOperation,SectigoCertificateManager.PowerShell.NewSectigoOrderCommand'
    }

    It 'Get-SectigoInventory throws in Admin mode' {
        Connect-Sectigo -ClientId 'id' -ClientSecret 'secret' | Out-Null

        { Get-SectigoInventory } |
            Should -Throw -ErrorId 'InvalidOperation,SectigoCertificateManager.PowerShell.GetSectigoInventoryCommand'
    }
}

