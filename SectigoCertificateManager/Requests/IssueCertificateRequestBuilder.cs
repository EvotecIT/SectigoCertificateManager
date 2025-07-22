namespace SectigoCertificateManager.Requests;

using System;
using System.Collections.Generic;
using System.Linq;
using SectigoCertificateManager.Utilities;

/// <summary>
/// Builds <see cref="IssueCertificateRequest"/> instances using a fluent API.
/// </summary>
public sealed class IssueCertificateRequestBuilder {
    private readonly IssueCertificateRequest _request = new();
    private readonly IReadOnlyList<int> _allowedTerms;
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="IssueCertificateRequestBuilder"/> class.
    /// </summary>
    /// <param name="allowedTerms">List of allowed certificate terms.</param>
    public IssueCertificateRequestBuilder(IReadOnlyList<int> allowedTerms) {
        _allowedTerms = Guard.AgainstNull(allowedTerms, nameof(allowedTerms));
    }

    /// <summary>Sets the certificate common name.</summary>
    public IssueCertificateRequestBuilder WithCommonName(string commonName) {
        lock (_lock) {
            _request.CommonName = commonName;
        }
        return this;
    }

    /// <summary>Sets the profile identifier.</summary>
    public IssueCertificateRequestBuilder WithProfileId(int profileId) {
        lock (_lock) {
            _request.ProfileId = profileId;
        }
        return this;
    }

    /// <summary>Sets the certificate term.</summary>
    public IssueCertificateRequestBuilder WithTerm(int term) {
        if (!_allowedTerms.Contains(term)) {
            throw new ArgumentOutOfRangeException(nameof(term));
        }

        lock (_lock) {
            _request.Term = term;
        }
        return this;
    }

    /// <summary>Sets subject alternative names.</summary>
    public IssueCertificateRequestBuilder WithSubjectAlternativeNames(IEnumerable<string> sans) {
        lock (_lock) {
            _request.SubjectAlternativeNames = sans?.ToArray() ?? Array.Empty<string>();
        }
        return this;
    }

    /// <summary>Builds the request instance.</summary>
    public IssueCertificateRequest Build() {
        lock (_lock) {
            return new IssueCertificateRequest {
                CommonName = _request.CommonName,
                ProfileId = _request.ProfileId,
                Term = _request.Term,
                SubjectAlternativeNames = _request.SubjectAlternativeNames.ToArray()
            };
        }
    }
}
