Describe 'Test-SectigoCsr' -Tag 'Cmdlet' {
    It 'returns true for valid CSR' {
        $valid = 'MIICVDCCATwCAQAwDzENMAsGA1UEAwwEVGVzdDCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEBAMwJzo64p670+fpPa/aEdbFBZJj1BBhOwhw7hHYrPp64mriZFjRfd7mIHaXpoXJ1ZqZm2wjovwh/ZSKv25LD8FaN83RrM175fAl1h3VAs31UE0yl56AUjs2mpMZXiU8E65wmfTdTuy6MhcDziGVmniasL9FC6gt05j2dSaNCZjTEUhP3Nv7abWU1eDuMZ0QdDqrMZXmiS6FaOIZfW4zg0X+oHLFoiy8hF2wa0yg5OcwxtTcrmQhCQxn3GxkZUQFZavhJUGydCFrtOyFULUCZHnDvfCyxZ8duwQCS/2ilOar1SrZ7AGcypV0yXH19LX/WjORdHb7CagDsPCzMQjsvhAECAwEAAaAAMA0GCSqGSIb3DQEBCwUAA4IBAQBWX3mWclTQvnak1cb2LR56QLsTCr9BmRLv9OatWcYG7P7aAJnpIJn57EJj16yBcukvQhmNz6TMMMnWPe6PeUmxB5FIvN7xhcvwVqGGB37SS6GBJOOQb70OAmuwe3plHMNR7Wk6bb/9S2+NYA8KNNXKymGSFhBFDSjC1cU3n7dqE5Smx1Gt2MMcNhidPeJWuxYUugEBtNglqO3sjRRVPV1Ybj1egVSbqxMw2bsGQBAsdSmPTkD0T61nkpVotexRmnts/8D30t744FGJeW1GoCREsy3/c9XJLQPkfJVKWCmLXfKo7p8HjgvbQcDqqmH3yv9vF97dDKAStz/mxxVzsEpr'
        Test-SectigoCsr -Csr $valid | Should -BeTrue
    }
    It 'returns false for invalid CSR' {
        $invalid = 'MIICVDCCATwCAQAwDzENMAsGA1UEAwwEVGVzdA=='
        Test-SectigoCsr -Csr $invalid | Should -BeFalse
    }
}
