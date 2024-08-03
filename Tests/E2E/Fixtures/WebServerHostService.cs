using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Builders;

namespace Tests;

public class WebServerHostService : IAsyncLifetime
    {
        public readonly ICompositeService Host = new Builder()
            .UseContainer()
            .UseCompose()
            .FromFile("../../docker-compose.yaml")
            .ForceBuild()
            .ForceRecreate()
            .KeepRunning()
            .Build();

        public async Task DisposeAsync()
        {
            await Task.Run(() =>
            {
                Host.Dispose();
            });
        }

        public async Task InitializeAsync()
        {
            Host.Start();
            await Task.Delay(TimeSpan.FromSeconds(5));
        }
    }
