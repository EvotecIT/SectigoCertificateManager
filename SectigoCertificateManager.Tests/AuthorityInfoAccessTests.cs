using SectigoCertificateManager.Utilities;
using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
#if !NET8_0_OR_GREATER
using System.Formats.Asn1;
#endif
using Xunit;

namespace SectigoCertificateManager.Tests;

/// <summary>
/// Tests for <see cref="X509Certificate2Extensions.GetAuthorityInfoAccess"/>.
/// </summary>
public sealed class AuthorityInfoAccessTests
{
    private static X509Certificate2 CreateCertificate()
    {
        using var key = RSA.Create(2048);
#if NET472
        var req = new CertificateRequest(
            new X500DistinguishedName("CN=Test"),
            key,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);
#else
        var req = new CertificateRequest("CN=Test", key, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
#endif
#if NET8_0_OR_GREATER
        var ext = new X509AuthorityInformationAccessExtension(
            new[] { "http://ocsp.test" },
            new[] { "http://ca.test" },
            false);
        req.CertificateExtensions.Add(ext);
#else
        var writer = new AsnWriter(AsnEncodingRules.DER);
        writer.PushSequence();
        writer.PushSequence();
        writer.WriteObjectIdentifier("1.3.6.1.5.5.7.48.1");
        writer.WriteCharacterString(UniversalTagNumber.IA5String, "http://ocsp.test", new Asn1Tag(TagClass.ContextSpecific, 6));
        writer.PopSequence();
        writer.PushSequence();
        writer.WriteObjectIdentifier("1.3.6.1.5.5.7.48.2");
        writer.WriteCharacterString(UniversalTagNumber.IA5String, "http://ca.test", new Asn1Tag(TagClass.ContextSpecific, 6));
        writer.PopSequence();
        writer.PopSequence();
        var data = writer.Encode();
        req.CertificateExtensions.Add(new X509Extension("1.3.6.1.5.5.7.1.1", data, false));
#endif
        return req.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(1));
    }

    /// <summary>Parses AuthorityInfoAccess extension.</summary>
    [Fact]
    public void GetAuthorityInfoAccess_ReturnsUris()
    {
        using var cert = CreateCertificate();

        var aia = cert.GetAuthorityInfoAccess();

        Assert.Contains("http://ocsp.test", aia.OcspUris);
        Assert.Contains("http://ca.test", aia.CaIssuerUris);
    }
}
