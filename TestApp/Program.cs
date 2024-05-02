using System;
using Tilework.LoadBalancing.Haproxy;
using Tilework.LoadBalancing.Services;


var cfg = new HAProxyConfigurator();

var lb = new LoadBalancerService(cfg);
lb.ApplyConfiguration();
