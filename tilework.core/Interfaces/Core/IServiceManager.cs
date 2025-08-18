namespace Tilework.Core.Interfaces;

public interface IServiceManager
{
    public void StartService(string serviceName);
    public void StopService(string serviceName);
    public void RestartService(string serviceName);
    public void ReloadService(string serviceName);
    public bool IsServiceActive(string serviceName);
}