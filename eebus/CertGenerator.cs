using System;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace eebus;

public static class CertGenerator
{
    public static X509Certificate2 CreateSelfSignedCertificate(string deviceName,string? password = null)
    {
        var directory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),".eebus");
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var fileName = Path.Combine(directory, $"{deviceName}.pfx");
        if (File.Exists(fileName))
        {
            return X509CertificateLoader.LoadPkcs12FromFile(fileName,password); 
            //return new X509Certificate2(fileName);
        }

        using var ecdsa = ECDsa.Create(ECCurve.NamedCurves.nistP256);
        var request = new CertificateRequest(
            $"CN={deviceName}",
            ecdsa, HashAlgorithmName.SHA256);

        // SHIP identifies devices by their Subject Key Identifier (SKI)
        request.CertificateExtensions.Add(
            new X509SubjectKeyIdentifierExtension(request.PublicKey, false));

        // Subject Alternative Name
        SubjectAlternativeNameBuilder subjectAlternativeNameBuilder = new();
        subjectAlternativeNameBuilder.AddDnsName(deviceName);
        request.CertificateExtensions.Add(subjectAlternativeNameBuilder.Build());

        // add key usage
        request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DataEncipherment | X509KeyUsageFlags.KeyEncipherment | X509KeyUsageFlags.DigitalSignature, false));

        // Create the cert valid for 10 years
        var cert = request.CreateSelfSigned(
            DateTimeOffset.Now.AddDays(-1),
            DateTimeOffset.Now.AddYears(10));

        File.WriteAllBytes(fileName, cert.Export(X509ContentType.Pfx, password));
        return cert;
    }
}