using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureFunctionsApp.Repository;
using AzureFunctionsApp.Repository.IRepository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        services.AddHttpClient();

        // Inyectar el TelemetryClient de Application Insights
        services.AddApplicationInsightsTelemetryWorkerService();

        services.AddSingleton<ISendEmailRepository, SendEmailRepository>();

        string keyVaultUri = Environment.GetEnvironmentVariable("AzureFunctionAppKeyVaultUri");
        if (!string.IsNullOrEmpty(keyVaultUri))
        {
            services.AddSingleton(new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential()));
        }
    })
    .Build();

host.Run();
