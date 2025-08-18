using Tilework.CertificateManagement.Interfaces;

namespace Tilework.CertificateManagement.Models;

public class LetsEncryptConfiguration : AcmeConfiguration
{
    public LetsEncryptConfiguration() : base()
    {
#if DEBUG
        DirectoryUrl = "https://acme-staging-v02.api.letsencrypt.org/";
#else
        DirectoryUrl = "https://acme-v02.api.letsencrypt.org/";
#endif
    }

}