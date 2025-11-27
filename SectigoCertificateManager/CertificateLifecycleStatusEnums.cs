namespace SectigoCertificateManager;

/// <summary>
/// Discovery status filter for certificate search.
/// </summary>
public enum DiscoveryStatus {
    /// <summary>Certificate discovery has not been deployed.</summary>
    NotDeployed,
    /// <summary>Certificate discovery has been deployed.</summary>
    Deployed
}

/// <summary>
/// Install status filter for certificate search.
/// </summary>
public enum InstallStatus {
    /// <summary>No installation workflow configured.</summary>
    NotConfigured,
    /// <summary>Installation has not started.</summary>
    NotStarted,
    /// <summary>Key generation is in progress.</summary>
    KeyProcessing,
    /// <summary>Key and CSR are ready.</summary>
    KeyAndCsrReady,
    /// <summary>Certificate processing is underway.</summary>
    CertificateProcessing,
    /// <summary>Installation has been scheduled.</summary>
    InstallationScheduled,
    /// <summary>Installation is currently processing.</summary>
    InstallationProcessing,
    /// <summary>User action is required to proceed.</summary>
    ActionRequired,
    /// <summary>Ready for installation.</summary>
    ReadyForInstall,
    /// <summary>Server restart is required to complete installation.</summary>
    ServerRestartRequired,
    /// <summary>Installation completed successfully.</summary>
    Completed,
    /// <summary>Configuration is invalid.</summary>
    InvalidConfiguration
}

/// <summary>
/// Renewal status filter for certificate search.
/// </summary>
public enum RenewalStatus {
    /// <summary>No renewal has been scheduled.</summary>
    NotScheduled,
    /// <summary>Renewal has been scheduled.</summary>
    Scheduled,
    /// <summary>Renewal has started.</summary>
    Started,
    /// <summary>Renewal finished successfully.</summary>
    Successful,
    /// <summary>Renewal failed.</summary>
    Failed
}
