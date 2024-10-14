using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using AzureFunctionsApp.Repository.IRepository;
using AzureFunctionsApp.Repository;
using Microsoft.Extensions.Logging;
using AzureFunctionsApp;
using OceansApp.Utility.LazyLoading;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.DataAccess.Repository;
using Microsoft.EntityFrameworkCore;
using OceansApp.DataAccess.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.ApplicationInsights;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults(worker =>
    {
        worker.UseMiddleware<ExceptionHandlingMiddleware>();
    })
    .ConfigureLogging(logging =>
    {
        logging.AddApplicationInsights();
        logging.AddConsole();
    })
    .ConfigureServices(services =>
    {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        string connectionString = Environment.GetEnvironmentVariable("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

        services.AddMemoryCache();

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ISlackRepository, SlackRepository>();
        services.AddScoped<IAzureBlobRepository, AzureBlobRepository>();
        services.AddScoped<IProjectConsultantAssignedHistoryRepository, ProjectConsultantAssignedHistoryRepository>();

        string keyVaultUri = Environment.GetEnvironmentVariable("AzureKeyVaultUri");
        if (!string.IsNullOrEmpty(keyVaultUri))
        {
            services.AddSingleton(new SecretClient(new Uri(keyVaultUri), new DefaultAzureCredential()));
        }

        services.AddSingleton<ISendEmailRepository>(provider =>
        {
            var secretClient = provider.GetRequiredService<SecretClient>();
            var telemetryClient = provider.GetRequiredService<TelemetryClient>();
            return new SendEmailRepository(secretClient, telemetryClient);
        });

        services.AddSingleton(typeof(LazyServiceProvider<>), typeof(LazyServiceProvider<>));
    })
    .Build();

host.Run();

