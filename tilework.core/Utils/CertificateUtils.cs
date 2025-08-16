using System.Text.RegularExpressions;
using System.Text;
using System.Security.Cryptography.X509Certificates;

namespace Tilework.Core.Utils;


public static class CertificateUtils
{
    public static List<X509Certificate2> LoadPemChain(string pemString)
    {
        var regex = new Regex(
            "-----BEGIN CERTIFICATE-----.*?-----END CERTIFICATE-----",
            RegexOptions.Singleline);

        var certs = new List<X509Certificate2>();

        foreach (Match match in regex.Matches(pemString))
        {
            var cert = match.Value.Trim();
            certs.Add(X509CertificateLoader.LoadCertificate(Encoding.UTF8.GetBytes(cert)));
        }

        return certs;
    }


    public static List<X509Certificate2> LoadPemChain(byte[] pemBuffer)
    {
        string pemString = Encoding.UTF8.GetString(pemBuffer);
        return LoadPemChain(pemString);
    }
}