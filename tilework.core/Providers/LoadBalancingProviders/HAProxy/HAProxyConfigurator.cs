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
using Tilework.Monitoring.Enums;
using Tilework.Monitoring.Models;
using Tilework.Persistence.LoadBalancing.Models;
using Tilework.Monitoring.Services;
using Tilework.Exceptions.Core;

namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyConfigurator : BaseContainerProvider, ILoadBalancingConfigurator
{
    protected static string _serviceName = "haproxy";
    protected static string _moduleName = "loadbalancing";

    private readonly IContainerManager _containerManager;
    private readonly LoadBalancerConfiguration _settings;
    private readonly ICertificateManagementService _certificateManagementService;
    private readonly DataCollectorService _dataCollectorService;
    private readonly ILogger<HAProxyConfigurator> _logger;
    private readonly IMapper _mapper;

    public HAProxyConfigurator(IOptions<LoadBalancerConfiguration> settings,
                               IContainerManager containerManager,
                               ICertificateManagementService certificateManagementService,
                               DataCollectorService dataCollectorService,
                               ILogger<HAProxyConfigurator> logger,
                               IMapper mapper) : base(containerManager, logger, _moduleName, _serviceName, settings.Value.BackendImage)
    {
        _logger = logger;
        _settings = settings.Value;
        _certificateManagementService = certificateManagementService;
        _dataCollectorService = dataCollectorService;
        _containerManager = containerManager;
        _mapper = mapper;
    }

    public List<BaseLoadBalancer> LoadConfiguration()
    {
        return null;
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

    private async Task<List<ContainerFile>> GetCertificateFiles(BaseLoadBalancer loadBalancer)
    {
        var containerFiles = new List<ContainerFile>();

        var certlist = new StringBuilder();

        var activeCertificates = loadBalancer.Certificates.Where(c => c.PrivateKey != null);

        foreach (var cert in activeCertificates)
        {
            var certData = string.Join("\n", cert.CertificateData.Select(c => GetCertPem(c)));
            var keyData = GetPrivateKeyPem(cert.PrivateKey.KeyData);

            var keyType = cert.PrivateKey.KeyData is RSA ? "rsa" : "ecdsa";

            var certFilePath = Path.GetTempFileName();

            File.WriteAllText(certFilePath, $"{keyData}\n{certData}");

            var containerFile = new ContainerFile()
            {
                LocalPath = certFilePath,
                ContainerPath = $"/usr/local/etc/haproxy/certs/{cert.Fqdn}.{keyType}.pem"
            };

            containerFiles.Add(containerFile);

            certlist.Append($"{containerFile.ContainerPath}\n");
        }


        var certListFilePath = Path.GetTempFileName();
        File.WriteAllText(certListFilePath, certlist.ToString());

        containerFiles.Add(new ContainerFile()
        {
            LocalPath = certListFilePath,
            ContainerPath = "/usr/local/etc/haproxy/certs/certlist.txt"
        });

        return containerFiles;
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


    public async Task ConfigureMonitoring(BaseLoadBalancer loadBalancer)
    {
        if (loadBalancer.Enabled == true && _dataCollectorService.IsMonitored(loadBalancer.Id.ToString()) == false)
        {
            var monitoringSource = new MonitoringSource()
            {
                Module = "LoadBalancing",
                Name = loadBalancer.Id.ToString(),
                Type = MonitoringSourceType.HAPROXY,
                Host = Host.Parse(await GetLoadBalancerHostname(loadBalancer)),
                Port = 4380
            };
            await _dataCollectorService.StartMonitoring(monitoringSource);
        }
        else if (loadBalancer.Enabled == false && _dataCollectorService.IsMonitored(loadBalancer.Id.ToString()) == true)
        {
            await _dataCollectorService.StopMonitoring(loadBalancer.Id.ToString());
        }
    }

    public async Task ApplyConfiguration(BaseLoadBalancer loadBalancer)
    {
            if(loadBalancer.Enabled == true)
            {
                var port = new ContainerPort()
                {
                    Port = loadBalancer.Port,
                    HostPort = loadBalancer.Port,
                    Type = _mapper.Map<PortType>(loadBalancer)
                };


                var localConfigPath = Path.GetTempFileName();
                var configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "haproxy.cfg");

                if (!File.Exists(configPath))
                    throw new InvalidOperationException($"No default haproxy configuration file found at {configPath}");

                var containerFiles = new List<ContainerFile>();

                try
                {
                    File.Copy(configPath, localConfigPath, overwrite: true);
                    UpdateConfigFile(localConfigPath, loadBalancer);

                    containerFiles.Add(new ContainerFile()
                    {
                        LocalPath = localConfigPath,
                        ContainerPath = "/usr/local/etc/haproxy/haproxy.cfg"
                    });

                    containerFiles.AddRange(await GetCertificateFiles(loadBalancer));

                    await StartUp(loadBalancer.Name, new() { port }, containerFiles, ContainerRestartType.SIGNAL);
                }
                catch(DockerException ex)
                {
                    if(ex.Type == ContainerExceptionType.PORT_CONFLICT)
                    {
                        throw new PortConfictException("Port is already in use");
                    }
                    throw;
                }
                finally
                {
                    foreach(var file in containerFiles)
                    {
                        if (File.Exists(file.LocalPath))
                            File.Delete(file.LocalPath);
                    }
                }
            }
            else
            {
                await Shutdown(loadBalancer.Name);
            }

            
            await ConfigureMonitoring(loadBalancer);
    }

    public async Task ApplyConfiguration(List<BaseLoadBalancer> loadBalancers)
    {
        foreach(var lb in loadBalancers)
        {
            try
            {
                await ApplyConfiguration(lb);
            }
            catch(Exception ex)
            {
                _logger.LogCritical(ex, $"Failed to configure load balancer [{lb.Name}]");
            }
        }

        var containers = await GetContainers();
        var containersToDelete = containers.Where(cnt => !loadBalancers.Any(lb =>  GetFullName(lb.Name) == cnt.Name)).ToList();

        foreach (var cnt in containersToDelete)
        {
            await Shutdown(cnt.Name);
        }
    }

    private async Task<Container> GetContainer(BaseLoadBalancer balancer)
    {
        var container = await GetContainer(balancer.Name);
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
        var containers = await GetContainers();
        foreach (var cnt in containers)
        {
            await Shutdown(cnt.Name);
        }
    }
}