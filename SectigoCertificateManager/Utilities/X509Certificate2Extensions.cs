namespace SectigoCertificateManager.Utilities;

using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// Extension methods for <see cref="X509Certificate2"/>.
/// </summary>
public static class X509Certificate2Extensions
{
    private const string AuthorityInfoAccessOid = "1.3.6.1.5.5.7.1.1";
    private const string OcspOid = "1.3.6.1.5.5.7.48.1";
    private const string CaIssuersOid = "1.3.6.1.5.5.7.48.2";

    /// <summary>Gets AuthorityInfoAccess data from the certificate.</summary>
    public static AuthorityInfoAccess GetAuthorityInfoAccess(this X509Certificate2 certificate)
    {
        if (certificate is null)
        {
            throw new ArgumentNullException(nameof(certificate));
        }
#if NET8_0_OR_GREATER
        if (certificate.Extensions[AuthorityInfoAccessOid] is X509AuthorityInformationAccessExtension aia)
        {
            return new AuthorityInfoAccess
            {
                OcspUris = aia.EnumerateOcspUris().ToArray(),
                CaIssuerUris = aia.EnumerateCAIssuersUris().ToArray()
            };
        }
        return new AuthorityInfoAccess();
#else
        var ext = certificate.Extensions[AuthorityInfoAccessOid];
        if (ext is null)
        {
            return new AuthorityInfoAccess();
        }
        var ocsp = new List<string>();
        var issuers = new List<string>();
        var reader = new AsnReader(ext.RawData, AsnEncodingRules.DER);
        var seq = reader.ReadSequence();
        while (seq.HasData)
        {
            var ad = seq.ReadSequence();
            var method = ad.ReadObjectIdentifier();
            var uri = ad.ReadCharacterString(UniversalTagNumber.IA5String, new Asn1Tag(TagClass.ContextSpecific, 6));
            if (method == OcspOid)
            {
                ocsp.Add(uri);
            }
            else if (method == CaIssuersOid)
            {
                issuers.Add(uri);
            }
        }
        return new AuthorityInfoAccess
        {
            OcspUris = ocsp.ToArray(),
            CaIssuerUris = issuers.ToArray()
        };
#endif
    }
}
