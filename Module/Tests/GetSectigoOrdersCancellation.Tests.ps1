Describe 'Get-SectigoOrders supports cancellation' -Tag 'Cmdlet' {
    It 'Stops when cancellation token is cancelled' {
        $cts = [System.Threading.CancellationTokenSource]::new()
        $timer = [System.Timers.Timer]::new(50)
        Register-ObjectEvent -InputObject $timer -EventName Elapsed -SourceIdentifier 'cancel' -MessageData $cts -Action { $Event.MessageData.Cancel() } | Out-Null
        $timer.AutoReset = $false
        $timer.Enabled = $true

        $params = @{
            BaseUrl  = 'https://example.com'
            Username = 'user'
            Password = 'pass'
            CustomerUri = 'cust'
            CancellationToken = $cts.Token
        }
        { Get-SectigoOrders @params } | Should -Throw
        $timer.Dispose()
        Unregister-Event -SourceIdentifier 'cancel'
    }
}
