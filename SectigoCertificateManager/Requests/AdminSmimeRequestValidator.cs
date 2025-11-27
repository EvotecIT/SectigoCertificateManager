namespace SectigoCertificateManager.Requests;

using System;
using System.Net.Mail;

/// <summary>
/// Validation helpers for S/MIME Admin API request types.
/// </summary>
internal static class AdminSmimeRequestValidator {
    /// <summary>
    /// Validates an enrollment request and throws when required fields are missing or invalid.
    /// </summary>
    /// <param name="request">The enrollment request to validate.</param>
    public static void ValidateEnrollRequest(AdminSmimeEnrollRequest request) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.FirstName)) {
            throw new ArgumentException("FirstName cannot be null or whitespace in the request.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.LastName)) {
            throw new ArgumentException("LastName cannot be null or whitespace in the request.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Email)) {
            throw new ArgumentException("Email cannot be null or whitespace in the request.", nameof(request));
        }

        if (!IsValidEmail(request.Email)) {
            throw new ArgumentException("Email is not in a valid format in the request.", nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Csr)) {
            throw new ArgumentException("Csr cannot be null or whitespace in the request.", nameof(request));
        }

        if (request.CertType <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request), "CertType must be greater than zero in the request.");
        }

        if (request.Term <= 0) {
            throw new ArgumentOutOfRangeException(nameof(request), "Term must be greater than zero in the request.");
        }
    }

    /// <summary>
    /// Validates a P12 download request and throws when values are outside the Admin API contract.
    /// </summary>
    /// <param name="request">The P12 download request to validate.</param>
    public static void ValidateP12DownloadRequest(AdminSmimeP12DownloadRequest request) {
        if (request is null) {
            throw new ArgumentNullException(nameof(request));
        }

        if (!string.IsNullOrWhiteSpace(request.EncryptionType)
            && !AdminSmimeP12EncryptionTypes.IsSupported(request.EncryptionType!)) {
            throw new ArgumentException(
                "EncryptionType must match one of the supported Admin API values when specified.",
                nameof(request));
        }
    }

    private static bool IsValidEmail(string email) {
        try {
            _ = new MailAddress(email);
            return true;
        } catch (FormatException) {
            return false;
        } catch (ArgumentException) {
            return false;
        }
    }
}
