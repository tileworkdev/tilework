using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using AutoMapper;

using Tilework.LoadBalancing.Interfaces;
using Tilework.LoadBalancing.Models;

using Tilework.Core.Interfaces;
using Tilework.Core.Enums;
using Tilework.Core.Models;

using Tilework.CertificateManagement.Interfaces;
using Tilework.CertificateManagement.Enums;
using Tilework.Persistence.LoadBalancing.Models;

namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyConfigurator : ILoadBalancingConfigurator
{
    public string ServiceName => "HAProxy";

    private readonly IContainerManager _containerManager;
    private readonly LoadBalancerConfiguration _settings;
    private readonly ICertificateManagementService _certificateManagementService;
    private readonly ILogger<HAProxyConfigurator> _logger;
    private readonly IMapper _mapper;

    public HAProxyConfigurator(IOptions<LoadBalancerConfiguration> settings,
                               IContainerManager containerManager,
                               ICertificateManagementService certificateManagementService,
                               ILogger<HAProxyConfigurator> logger,
                               IMapper mapper)
    {
        _logger = logger;
        _settings = settings.Value;
        _certificateManagementService = certificateManagementService;
        _containerManager = containerManager;
        _mapper = mapper;
    }

    public List<BaseLoadBalancer> LoadConfiguration()
    {
        return null;
    }

    private async Task<List<Container>> GetLoadBalancerContainers()
    {
        return await _containerManager.ListContainers("loadbalancing.tile");
    }

    private void UpdateConfigFile(string path, BaseLoadBalancer balancer)
    {
        var haproxyConfig = new Configuration(path);
        haproxyConfig.Load();

        haproxyConfig.Frontends = new List<ConfigSection>();
        haproxyConfig.Backends = new List<ConfigSection>();

        var fe = _mapper.Map<FrontendSection>(balancer);
        haproxyConfig.Frontends.Add(fe);


        List<TargetGroup> targetGroups;

        if (balancer is ApplicationLoadBalancer appLoadBalancer)
        {
            targetGroups = appLoadBalancer.Rules != null ? appLoadBalancer.Rules.Select(r => r.TargetGroup).ToList() : new List<TargetGroup>();
        }
        else if (balancer is NetworkLoadBalancer netLoadBalancer)
        {
            targetGroups = new List<TargetGroup>() { netLoadBalancer.TargetGroup };
        }
        else
            throw new ArgumentException("Invalid load balancer type");


        haproxyConfig.Backends = targetGroups
            .GroupBy(tg => tg.Id).Select(g => g.First()) // Deduplicate target groups
            .Select(tg => (ConfigSection)_mapper.Map<BackendSection>(tg)) // map them to backend sections
            .ToList();

        haproxyConfig.Save();
    }

    private async Task SaveCertificates(Container container, BaseLoadBalancer loadBalancer)
    {
        var certlist = new StringBuilder();

        var activeCertificates = loadBalancer.Certificates.Where(
            c => c.Status == CertificateStatus.ACTIVE &&
            c.PrivateKey != null);

        foreach (var cert in activeCertificates)
        {
            var certData = string.Join("\n", cert.CertificateData.Select(c => GetCertPem(c)));
            var keyData = GetPrivateKeyPem(cert.PrivateKey.KeyData);

            var keyType = cert.PrivateKey.KeyData is RSA ? "rsa" : "ecdsa";

            var certFilePath = Path.GetTempFileName();

            var containerFilePath = $"/usr/local/etc/haproxy/certs/{cert.Fqdn}.{keyType}.pem";

            try
            {
                File.WriteAllText(certFilePath, $"{keyData}\n{certData}");
                await _containerManager.CopyFileToContainer(
                    container.Id,
                    certFilePath,
                    containerFilePath
                );
            }
            finally
            {
                if (File.Exists(certFilePath))
                    File.Delete(certFilePath);
            }

            certlist.Append($"{containerFilePath}\n");
        }

        var certListFilePath = Path.GetTempFileName();
        
        try
        {
            File.WriteAllText(certListFilePath, certlist.ToString());
            await _containerManager.CopyFileToContainer(
                container.Id,
                certListFilePath,
                "/usr/local/etc/haproxy/certs/certlist.txt"
            );
        }
        finally
        {
            if (File.Exists(certListFilePath))
                File.Delete(certListFilePath);
        }
    }

    public async Task ApplyConfiguration(List<BaseLoadBalancer> config)
    {
        if (string.IsNullOrEmpty(_settings.BackendImage))
            throw new ArgumentException("No image setting supplied for load balancing tile");

        var containers = await GetLoadBalancerContainers();
        foreach (var lb in config)
        {
            var name = lb.Id.ToString();
            var container = containers.FirstOrDefault(cnt => cnt.Name == name);

            if (container == null)
            {
                _logger.LogInformation($"Creating new container for load balancer {lb.Name}");
                var port = new ContainerPort()
                {
                    Port = lb.Port,
                    HostPort = lb.Port,
                    Type = _mapper.Map<PortType>(lb)
                };

                try
                {
                    container = await _containerManager.CreateContainer(
                        name,
                        _settings.BackendImage,
                        "loadbalancing.tile",
                        new List<ContainerPort>() { port }
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogCritical($"Failed to create container for load balancer {lb.Name}: {ex.ToString()}");
                    throw;
                }
            }

            var localConfigPath = Path.GetTempFileName();
            var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "haproxy.cfg");

            if (!File.Exists(configPath))
                throw new InvalidOperationException($"No default haproxy configuration file found at {configPath}");

            try
            {
                File.Copy(configPath, localConfigPath, overwrite: true);
                UpdateConfigFile(localConfigPath, lb);
                await _containerManager.CopyFileToContainer(container.Id, localConfigPath, "/usr/local/etc/haproxy/haproxy.cfg");
            }
            finally
            {
                if (File.Exists(localConfigPath))
                    File.Delete(localConfigPath);
            }

            await SaveCertificates(container, lb);


            if (container.State != ContainerState.Running)
            {
                if (lb.Enabled == true)
                {
                    _logger.LogInformation($"Starting container for load balancer {lb.Name}");
                    await _containerManager.StartContainer(container.Id);
                }
            }
            else
            {
                if (lb.Enabled == true)
                {
                    _logger.LogInformation($"Signaling container for load balancer {lb.Name} of configuration changes");
                    await _containerManager.KillContainer(container.Id, UnixSignal.SIGHUP);
                }
                else
                {
                    _logger.LogInformation($"Stopping container for load balancer {lb.Name}");
                    await _containerManager.StopContainer(container.Id);
                }
            }
        }

        var containersToDelete = containers.Where(cnt => !config.Any(lb => lb.Id.ToString() == cnt.Name)).ToList();
        foreach (var cnt in containersToDelete)
        {
            _logger.LogInformation($"Deleting load balancer {cnt.Name}");
            if (cnt.State == ContainerState.Running)
                await _containerManager.StopContainer(cnt.Id);
            await _containerManager.DeleteContainer(cnt.Id);
        }
    }

    private static string GetPrivateKeyPem(AsymmetricAlgorithm key)
    {
        var pkcs8 = key.ExportPkcs8PrivateKey();
        var pem = PemEncoding.Write("PRIVATE KEY", pkcs8);
        return new string(pem);
    }

    private static string GetCertPem(X509Certificate2 cert)
    {
        var der = cert.Export(X509ContentType.Cert);
        var pem = PemEncoding.Write("CERTIFICATE", der);
        return new string(pem);
    }

    private async Task<Container> GetContainer(BaseLoadBalancer balancer)
    {
        var containers = await GetLoadBalancerContainers();
        var container = containers.FirstOrDefault(cnt => cnt.Name == balancer.Id.ToString());
        if (container == null)
            throw new ArgumentException($"Container for balancer {balancer.Id} not found");
        return container;
    }

    public async Task<string> GetLoadBalancerHostname(BaseLoadBalancer balancer)
    {
        var container = await GetContainer(balancer);
        return (await _containerManager.GetContainerAddress(container.Id)).ToString();
    }


    public async Task<bool> CheckLoadBalancerStatus(BaseLoadBalancer balancer)
    {
        var container = await GetContainer(balancer);
        return container.State == ContainerState.Running;
    }

    public async Task Shutdown()
    {
        var containers = await GetLoadBalancerContainers();
        foreach (var cnt in containers)
        {
            _logger.LogInformation($"Stopping and deleting load balancer {cnt.Name}");
            if (cnt.State == ContainerState.Running)
                await _containerManager.StopContainer(cnt.Id);
            await _containerManager.DeleteContainer(cnt.Id);
        }
    }
}