namespace SectigoCertificateManager.Utilities;

using System.ComponentModel.DataAnnotations;

/// <summary>
/// Provides helper methods to validate request objects using data annotations.
/// </summary>
internal static class RequestValidator {
    public static void Validate(object request) {
        _ = request ?? throw new ArgumentNullException(nameof(request));
        var context = new ValidationContext(request);
        Validator.ValidateObject(request, context, validateAllProperties: true);
    }
}
