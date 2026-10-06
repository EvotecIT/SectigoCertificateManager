namespace SectigoCertificateManager.Tests;

/// <summary>Serializes tests using process configuration against all other test collections.</summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class ApiConfigEnvironmentCollection {
    public const string Name = "API configuration environment";
}
