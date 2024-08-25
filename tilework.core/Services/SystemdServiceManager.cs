using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

using Tilework.Core.Enums;
using Tilework.Core.Interfaces;

namespace Tilework.Core.Services;

public class SystemdServiceManager : IServiceManager
{
    private readonly ILogger<SystemdServiceManager> _logger;
    public SystemdServiceManager(ILogger<SystemdServiceManager> logger)
    {
        _logger = logger;
    }

    private void ManageService(ServiceManagerAction action, string serviceName)
    {
        string actionCommand;

        switch(action)
        {
            case ServiceManagerAction.Start:
                actionCommand = "start";
                break;
            case ServiceManagerAction.Stop:
                actionCommand = "stop";
                break;
            case ServiceManagerAction.Restart:
                actionCommand = "restart";
                break;
            default:
                throw new ArgumentException("Unkown service manager action received");
        }

        using (Process process = new Process())
        {
            process.StartInfo.FileName = "systemctl";
            process.StartInfo.Arguments = $"{actionCommand} {serviceName}"; // The systemctl command and its arguments
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.Start();

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                _logger.LogInformation($"Service {serviceName} {actionCommand}ed successfully.");
                _logger.LogInformation(output);
            }
            else
            {
                var result = $"Error {action}ing service {serviceName}: {error}";
                _logger.LogCritical(result);
                throw new Exception(result);
            }
        }
    }

    public void StartService(string serviceName) => ManageService(ServiceManagerAction.Start, serviceName);

    public void StopService(string serviceName) => ManageService(ServiceManagerAction.Stop, serviceName);

    public void RestartService(string serviceName) => ManageService(ServiceManagerAction.Restart, serviceName);
}