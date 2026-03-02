using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;


namespace eebus.Extensions;

internal static class CertificateExtensions
{
    public static string? GetSubjectKeyIdentifier(this X509Certificate2 cert)
    {
        ArgumentNullException.ThrowIfNull(cert);
        // cert is your X509Certificate2 instance
        var skiExtension = cert.Extensions
            .OfType<X509SubjectKeyIdentifierExtension>()
            .FirstOrDefault();

        return skiExtension?.SubjectKeyIdentifier;
    }
    public static string? GetSubjectKeyIdentifier(this X509Certificate cert)
    {
        ArgumentNullException.ThrowIfNull(cert);

        // If already X509Certificate2, delegate to the other overload
        if (cert is X509Certificate2 cert2)
            return GetSubjectKeyIdentifier(cert2);

        // Try converting to X509Certificate2 to access Extensions/PublicKey
        try
        {
            cert2 = new X509Certificate2(cert);
        }
        catch (CryptographicException)
        {
            // Conversion failed, cannot read extensions or public key reliably
            return null;
        }

        return GetSubjectKeyIdentifier(cert2);
    }
}
