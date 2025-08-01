Describe "Client disposal" {
    BeforeAll {
        dotnet build "$PSScriptRoot/../../SectigoCertificateManager.PowerShell" -c Release | Out-Null
        $psModule = Join-Path $PSScriptRoot '../../SectigoCertificateManager.PowerShell/bin/Release/net8.0/SectigoCertificateManager.PowerShell.dll'
        Import-Module $psModule
        $lib = Join-Path $PSScriptRoot '../../SectigoCertificateManager/bin/Release/net8.0/SectigoCertificateManager.dll'
        Add-Type -Path $lib
        Add-Type -AssemblyName System.Net.Http
        Add-Type -TypeDefinition @'
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
public sealed class NullHandler : HttpMessageHandler {
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        => Task.FromResult(new HttpResponseMessage(System.Net.HttpStatusCode.OK) { Content = new StringContent("[]") });
}
'@
        [SectigoCertificateManager.PowerShell.TestHooks]::ClientFactory = [System.Func[SectigoCertificateManager.ApiConfig,SectigoCertificateManager.ISectigoClient]]{
            param($cfg)
            $handler = [NullHandler]::new()
            $http = [System.Net.Http.HttpClient]::new($handler)
            [SectigoCertificateManager.SectigoClient]::new($cfg, $http)
        }
    }
    AfterAll {
        [SectigoCertificateManager.PowerShell.TestHooks]::ClientFactory = $null
        Remove-Module SectigoCertificateManager.PowerShell
    }
    It "disposes the client" {
        [SectigoCertificateManager.PowerShell.TestHooks]::CreatedClient = $null
        Get-SectigoCertificateTypes -BaseUrl 'https://example.com' -Username 'u' -Password 'p' -CustomerUri 'c'
        $client = [SectigoCertificateManager.PowerShell.TestHooks]::CreatedClient
        $client | Should -Not -BeNullOrEmpty
        $field = $client.GetType().GetField('_disposed', [System.Reflection.BindingFlags]::NonPublic -bor [System.Reflection.BindingFlags]::Instance)
        $field.GetValue($client) | Should -BeTrue
    }
}
