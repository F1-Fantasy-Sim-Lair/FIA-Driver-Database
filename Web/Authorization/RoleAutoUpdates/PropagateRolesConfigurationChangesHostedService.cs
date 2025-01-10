using Microsoft.Extensions.Options;
using System.Threading.Channels;

namespace Web.Authorization.RoleAutoUpdates;

class PropagateRolesConfigurationChangesHostedService(
    IOptionsMonitor<RolesConfiguration> monitorConfiguration,
    ChannelWriter<RolesConfiguration> writer) : IHostedService, IDisposable
{
    readonly IOptionsMonitor<RolesConfiguration> monitorConfiguration = monitorConfiguration;
    readonly ChannelWriter<RolesConfiguration> writer = writer;
    IDisposable? optionsMonitorListener;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        optionsMonitorListener = monitorConfiguration.OnChange(RolesConfigurationChanged);
        await writer.WriteAsync(monitorConfiguration.CurrentValue, cancellationToken);
    }

    void RolesConfigurationChanged(RolesConfiguration configuration)
    {
        if (!writer.TryWrite(configuration))
            throw new InvalidOperationException("Failed to write configuration change to channel");
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        optionsMonitorListener?.Dispose();
        return Task.CompletedTask;
    }

    #region IDisposable Support
    bool disposedValue;
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
                optionsMonitorListener?.Dispose();

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
    #endregion
}
