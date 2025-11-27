Describe 'Get-SectigoOrders supports cancellation' -Tag 'Cmdlet' {
    BeforeEach {
        try {
            Disconnect-Sectigo -ErrorAction SilentlyContinue
        } catch {
        }
    }

    It 'Stops when cancellation token is cancelled' {
        $cts = [System.Threading.CancellationTokenSource]::new()
        $timer = [System.Timers.Timer]::new(50)
        Register-ObjectEvent -InputObject $timer -EventName Elapsed -SourceIdentifier 'cancel' -MessageData $cts -Action { $Event.MessageData.Cancel() } | Out-Null
        $timer.AutoReset = $false
        $timer.Enabled = $true

        Connect-Sectigo -BaseUrl 'https://example.com' -Username 'user' -Password 'pass' -CustomerUri 'cust' -ApiVersion 'V25_6' | Out-Null

        { Get-SectigoOrders -CancellationToken $cts.Token } | Should -Throw

        $timer.Dispose()
        Unregister-Event -SourceIdentifier 'cancel'
    }
}
