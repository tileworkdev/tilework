using Microsoft.Extensions.Logging;
using System.Net.Sockets;
using System.Text;
using System.Globalization;

using AutoMapper;
using CsvHelper;

using Tilework.LoadBalancing.Models;
using Tilework.LoadBalancing.Interfaces;
using Tilework.Persistence.LoadBalancing.Models;

using Tilework.Core.Interfaces;

namespace Tilework.LoadBalancing.Haproxy;

public class HAProxyMonitor : ILoadBalancingMonitor
{
    private readonly IContainerManager _containerManager;
    private readonly HAProxyConfigurator _configurator;
    private readonly ILogger<HAProxyMonitor> _logger;
    private readonly IMapper _mapper;

    public HAProxyMonitor(IContainerManager containerManager,
                          HAProxyConfigurator configurator,
                          ILogger<HAProxyMonitor> logger,
                          IMapper mapper)
    {
        _logger = logger;
        _configurator = configurator;
        _containerManager = containerManager;
        _mapper = mapper;
    }

    public async Task<List<T>> SendReceiveCommand<T>(NetworkStream stream, string command)
    {
        var cmd = Encoding.ASCII.GetBytes($"{command}\n");
        stream.Write(cmd, 0, cmd.Length);

        var buffer = new byte[65535];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        var response = Encoding.ASCII.GetString(buffer, 0, bytesRead);

        var lines = response.Split('\n')
                            .Where(l => !string.IsNullOrWhiteSpace(l))
                            .ToList();

        var headerLine = lines.FirstOrDefault(l => l.TrimStart().StartsWith("#"));
        if (headerLine is null) return new();

        var header = headerLine.TrimStart('#', ' ');
        var dataLines = lines.Where(l => !l.TrimStart().StartsWith("#"));

        var normalized = string.Join('\n', new[] { header }.Concat(dataLines));

        using var reader = new StringReader(normalized);
        var config = new CsvHelper.Configuration.CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = true,
            BadDataFound = null,
            MissingFieldFound = null,
            HeaderValidated = null,
            TrimOptions = CsvHelper.Configuration.TrimOptions.Trim,
            DetectColumnCountChanges = false
        };

        using var csv = new CsvReader(reader, config);

        var intOpts = csv.Context.TypeConverterOptionsCache.GetOptions<int?>();
        intOpts.NullValues.AddRange(new[] { "", "-", "NA" });

        return csv.GetRecords<T>().ToList();
    }


    public async Task<LoadBalancingStatistics> GetRealtimeStatistics(BaseLoadBalancer balancer)
    {
        if (await _configurator.CheckLoadBalancerStatus(balancer) == false)
            throw new ArgumentOutOfRangeException($"Cannot get statistics for balancer {balancer}: Balancer is not running");

        var hostname = await _configurator.GetLoadBalancerHostname(balancer);

        using var client = new TcpClient(hostname, 4380);
        using var stream = client.GetStream();

        var response = await SendReceiveCommand<HAProxyStatisticsRow>(stream, "show stat");

        var balancerStats = response.First(r => r.svname == "FRONTEND" && r.pxname == balancer.Id.ToString());

        return _mapper.Map<LoadBalancingStatistics>(balancerStats);
    }
}