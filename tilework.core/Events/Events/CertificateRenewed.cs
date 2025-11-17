using Coravel.Events.Interfaces;

using Tilework.CertificateManagement.Models;

namespace Tilework.Events;

public class CertificateRenewed : IEvent
{
    public CertificateDTO Certificate { get; set; }

    public CertificateRenewed(CertificateDTO certificate) => Certificate = certificate;
}
