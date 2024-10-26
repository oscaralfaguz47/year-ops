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

var environment = builder.Environment.EnvironmentName;

// App Configuration connection string for accessing secrets from Key Vault
var appConfigConnectionString = Environment.GetEnvironmentVariable(builder.Configuration.GetConnectionString("AppConfigConnectionString"));

builder.Configuration.AddAzureAppConfiguration(options =>
{
    options.Connect(appConfigConnectionString)
           .ConfigureKeyVault(kv =>
           {
               kv.SetCredential(new DefaultAzureCredential());
           });

    options.Select(KeyFilter.Any, LabelFilter.Null)
    .ConfigureRefresh(refreshOptions =>
    {
        //reload all configuration in real time
        refreshOptions.Register("AppSettings:RefreshTrigger", refreshAll: true)
                      .SetCacheExpiration(TimeSpan.FromSeconds(30)); 
    });
});


// Retrieve the database connection string from Azure App Configuration or environment variable in Development
string connectionString;
if (environment == "Development") // Local
{
    // Try to get the DatabaseConnectionString from App Configuration (Key Vault via App Configuration)
    connectionString = Environment.GetEnvironmentVariable(builder.Configuration.GetConnectionString("DefaultConnection"));

    // Fall back to local environment variable if not set in App Configuration
    if (string.IsNullOrEmpty(connectionString))
    {
        connectionString = Environment.GetEnvironmentVariable(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
}
else
{
    // For Production/Demo, always use the connection string from App Configuration
    connectionString = builder.Configuration["DbConnectionString"];
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
    options.AddServerHeader = false;
});
// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin", builder => builder.WithOrigins("https://oceansapp.azurewebsites.net/"));
});

//Bonusly
builder.Services.AddHttpClient<BonuslyRepository>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(30);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{

});


// Background services
//builder.Services.AddHostedService<EveryOneDayServices>();

builder.Services.AddMemoryCache();
builder.Services.AddScoped<RequireTwoFactorEnabledAttribute>();
builder.Services.AddApplicationInsightsTelemetry();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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

async Task SeedDatabaseAsync(IHost app)
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        await dbInitializer.InitializeAsync();
    }
}