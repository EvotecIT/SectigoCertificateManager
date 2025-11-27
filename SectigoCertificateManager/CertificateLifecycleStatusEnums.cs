namespace SectigoCertificateManager;

/// <summary>
/// Discovery status filter for certificate search.
/// </summary>
public enum DiscoveryStatus {
    NotDeployed,
    Deployed
}

/// <summary>
/// Install status filter for certificate search.
/// </summary>
public enum InstallStatus {
    NotConfigured,
    NotStarted,
    KeyProcessing,
    KeyAndCsrReady,
    CertificateProcessing,
    InstallationScheduled,
    InstallationProcessing,
    ActionRequired,
    ReadyForInstall,
    ServerRestartRequired,
    Completed,
    InvalidConfiguration
}

/// <summary>
/// Renewal status filter for certificate search.
/// </summary>
public enum RenewalStatus {
    NotScheduled,
    Scheduled,
    Started,
    Successful,
    Failed
}

