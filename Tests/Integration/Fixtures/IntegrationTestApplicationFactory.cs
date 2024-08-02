using Ductus.FluentDocker.Services;
using Ductus.FluentDocker.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Ductus.FluentDocker;
using Microsoft.AspNetCore.TestHost;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tests;

public class IntegrationTestApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly IContainerImageService _dbImage;
    private readonly IContainerService _dbContainer;

    public IntegrationTestApplicationFactory()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddUserSecrets<IntegrationTestApplicationFactory>()
            .Build();

        string MSSQL_SA_PASSWORD = configuration["SA_PASSWORD"]
            ?? throw new ApplicationException("No password set for the test MSSQL database admin.");

        _dbImage = new Builder()
            .DefineImage("hotdesk_integrationtestdb")
            .BuildArguments("password=" + MSSQL_SA_PASSWORD)
            .FromFile("../../../../Database/Dockerfile")
            .Build();
        _dbContainer = new Builder()
            .UseContainer()
            .DeleteIfExists(force: true)
            .UseImage("hotdesk_integrationtestdb")
            .WithName("hotdesk_integrationtestdb")
            .ExposePort(14331,1433)
            .WaitForHealthy()
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
    }

    public async Task InitializeAsync()
    {
        await Task.Run(() =>
        {
            _dbContainer.Start();
        });
    }
    public new async Task DisposeAsync()
    {
        await Task.Run(() =>
        {
            _dbContainer.Dispose();
        });
    }
}