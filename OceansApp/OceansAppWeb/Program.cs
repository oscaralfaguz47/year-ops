using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OceansAppWeb;
using OceansApp.DataAccess.Data;
using OceansApp.DataAccess.Repository.IRepository;
using OceansApp.DataAccess.DbInitializer;
using OceansApp.DataAccess.Repository;
using Microsoft.AspNetCore.Http.Features;
using OceansApp.Utility.Configuration;
using OceansApp.Utility.LazyLoading;
using OceansApp.Utility;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using OceansApp.DataAccess;
using Azure.Identity;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;

var builder = WebApplication.CreateBuilder(args);

var logger = LoggerFactory.Create(config =>
{
    config.AddConsole();
    config.AddAzureWebAppDiagnostics(); 
}).CreateLogger("ProgramLogger");


try
{
    // Add services to the container.
    System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "RIPPLE API", Version = "v1" });
        c.CustomOperationIds(apiDesc =>
        {
            return apiDesc.TryGetMethodInfo(out MethodInfo methodInfo) ? methodInfo.Name : null;
        });
        c.OperationFilter<FormDataOperationFilter>();
        c.OperationFilter<AddAntiforgeryTokenHeaderParameter>();
    });


    builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

    // Get the current environment (Development, Production, etc.)
    var environment = builder.Environment.EnvironmentName;
    logger.LogInformation("Environment: {Environment}", environment);

    // App Configuration connection string for accessing secrets from Key Vault
    // Ensure the environment variable "AppConfigConnectionString" is correctly set.
    var appConfigConnectionString = Environment.GetEnvironmentVariable("AppConfigConnectionString");

    if (!string.IsNullOrEmpty(appConfigConnectionString))
    {
        logger.LogInformation("App Configuration connection string found. Configuring Azure App Configuration...");
        // Configure Azure App Configuration with connection to Key Vault
        builder.Configuration.AddAzureAppConfiguration(options =>
        {
            options.Connect(appConfigConnectionString)
                   .ConfigureKeyVault(kv =>
                   {
                       kv.SetCredential(new DefaultAzureCredential()); // Use managed identity for authentication
                   });

            // Select all keys and configure real-time refresh
            options.Select(KeyFilter.Any, LabelFilter.Null)
                   .ConfigureRefresh(refreshOptions =>
                   {
                       // Use a key "AppSettings:RefreshTrigger" to trigger real-time updates
                       refreshOptions.Register("AppSettings:RefreshTrigger", refreshAll: true)
                                     .SetCacheExpiration(TimeSpan.FromSeconds(30));
                   });
        });
    }
    else
    {
        logger.LogWarning("App Configuration connection string is missing or not set in environment variables.");
    }

    // Retrieve the database connection string depending on the environment
    string connectionString;

    if (environment == "Development" || environment == "Test")
    {
        // Trying to get from environment variables
        connectionString = Environment.GetEnvironmentVariable("DefaultConnection");


        logger.LogInformation("Using connection string from environment variables or local config.");
    }
    else
    {
        // Production if use App Configuration
        connectionString = builder.Configuration["DbConnectionString"];
        logger.LogInformation("Using connection string from Azure App Configuration.");
    }

    if (string.IsNullOrEmpty(connectionString))
    {
        logger.LogError("Database connection string could not be found.");
    }
    else
    {
        logger.LogInformation("Database connection string found and applied.");
    }

    // Configure DbContext for all environments (Development, Production, and Demo)
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));

    // Register DatabaseService if needed
    builder.Services.AddTransient<DatabaseService>(provider =>
        new DatabaseService(connectionString));

    builder.Services.AddIdentity<IdentityUser, IdentityRole>()
        .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();

    //Configure Authorization Policies
    AuthorizationConfig.ConfigurePolicies(builder.Services);

    // Configuring azureFormRecognizer - OCR
    builder.Services.AddSingleton<IOcrServiceRepository>(provider =>
    {
        var cfg = provider.GetRequiredService<IConfiguration>();
        var endpoint = cfg["AzureFormRecognizer:Endpoint"];
        var key = cfg["AzureFormRecognizer:Key"];
        return new OcrServiceRepository(endpoint, key);
    });
    //OpenAI
    builder.Services.AddScoped<IOpenAIRepository, OpenAIRepository>();
    //Reports validation
    builder.Services.AddScoped<IHourReportValidationServiceRepository, HourReportValidationServiceRepository>();

    // Scoped services
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IDbInitializer, DbInitializer>();
    builder.Services.AddScoped<ISlackRepository, SlackRepository>();
    builder.Services.AddScoped<IAzureBlobRepository, AzureBlobRepository>();
    builder.Services.AddScoped<IProjectConsultantAssignedHistoryRepository, ProjectConsultantAssignedHistoryRepository>();

    // Lazy loading configuration for scoped services
    builder.Services.AddScoped(typeof(Lazy<>), typeof(LazyServiceProvider<>));

    // Configuring QueueClient for Azure Queue Storage
    string queueConnectionString = builder.Configuration["AzureWebJobsStorage"];
    builder.Services.AddSingleton(_ => new QueueClient(queueConnectionString, "emailqueue"));


    builder.Services.Configure<IdentityOptions>(opt =>
    {
        opt.Password.RequiredLength = 8;
        opt.Password.RequireLowercase = true;
        opt.Password.RequireUppercase = true;
        opt.Password.RequireDigit = true;
        opt.Password.RequireNonAlphanumeric = true;
        opt.Password.RequiredUniqueChars = 6;
        opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
        opt.Lockout.MaxFailedAccessAttempts = 7;
    });

    builder.Services.Configure<FormOptions>(options =>
    {
        options.ValueCountLimit = int.MaxValue;
        options.ValueLengthLimit = int.MaxValue;
        options.MultipartBodyLengthLimit = long.MaxValue;
    });

    builder.Services.Configure<CookiePolicyOptions>(options =>
    {
        options.CheckConsentNeeded = context => false;
        options.MinimumSameSitePolicy = SameSiteMode.None;
    });

    builder.Services.ConfigureApplicationCookie(options =>
    {
        options.Cookie.HttpOnly = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // secured cookies on production 
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.IsEssential = true;
        options.ExpireTimeSpan = TimeSpan.FromMinutes(SD.SessionExpirationTime);
        options.SlidingExpiration = true;
        options.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Home/AccessDenied");
        options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
    });

    builder.Services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(30);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    });
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(5);
        options.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(5);
        options.Limits.MaxRequestBodySize = 104857600L;
        options.AddServerHeader = false;
    });
    // CORS configuration
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowSpecificOrigin", builder => builder.WithOrigins("https://www.ripple.oceanscode.com/"));
    });

    //Bonusly
    builder.Services.AddHttpClient<BonuslyRepository>(client =>
    {
        client.Timeout = TimeSpan.FromSeconds(30);
    }).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
    {

    });


    builder.Services.AddMemoryCache();
    builder.Services.AddScoped<RequireTwoFactorEnabledAttribute>();
    builder.Services.AddApplicationInsightsTelemetry();

    logger.LogInformation("Finished configuring services.");

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error");
        app.UseHsts();
    }

    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "RIPPLE API v1"));

    app.UseHttpsRedirection();
    app.UseRouting();
    await SeedDatabaseAsync(app);
    app.UseMiddleware<RedirectToDashboardMiddleware>();

    app.UseSession();
    app.UseCookiePolicy();
    app.UseAuthentication();
    app.UseCors("AllowSpecificOrigin");
    app.UseAuthorization();


    app.MapControllers();
    app.UseStaticFiles();

    app.MapRazorPages();
    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "areas",
            pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    });

    await app.RunAsync();

}
catch (Exception ex)
{
    logger.LogError(ex, "An error occurred during application startup");
}

async Task SeedDatabaseAsync(IHost app)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await dbInitializer.InitializeAsync();
    }
}