namespace SectigoCertificateManager.Utilities;

using System;
using System.Formats.Asn1;
using System.Security.Cryptography;

/// <summary>
/// Provides helpers for validating certificate signing requests.
/// </summary>
public static class CsrValidator {
    /// <summary>
    /// Determines whether the specified CSR is structurally valid.
    /// </summary>
    /// <param name="csr">Base64-encoded certificate signing request.</param>
    /// <returns><c>true</c> when the CSR is valid; otherwise, <c>false</c>.</returns>
    public static bool IsValid(string csr) {
        Guard.AgainstNullOrWhiteSpace(csr, nameof(csr));
        try {
            var data = Convert.FromBase64String(csr);
            var reader = new AsnReader(data, AsnEncodingRules.DER);
            var sequence = reader.ReadSequence();
            sequence.ReadSequence();
            sequence.ReadSequence();
            sequence.ReadBitString(out _);
            return !sequence.HasData && !reader.HasData;
        } catch (FormatException) {
            return false;
        } catch (AsnContentException) {
            return false;
        } catch (CryptographicException) {
            return false;
        }
    }
}
