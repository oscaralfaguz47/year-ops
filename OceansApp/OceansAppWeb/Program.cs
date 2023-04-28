using FinancialCalculator.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.EntityFrameworkCore;
using OceansAppWeb;
using OceansAppWeb.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
    builder.Configuration.GetConnectionString("DefaultConnnection")
    ));
builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>().AddDefaultTokenProviders();
builder.Services.AddSingleton<IEmailSender, EmailSender>();
builder.Services.Configure<IdentityOptions>(opt =>
{
    opt.Password.RequiredLength = 8;
    opt.Password.RequireLowercase = true;
    opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30);
    opt.Lockout.MaxFailedAccessAttempts = 3;
});
builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.AccessDeniedPath = new Microsoft.AspNetCore.Http.PathString("/Home/AccessDenied");
    opt.LoginPath = new Microsoft.AspNetCore.Http.PathString("/Account/Login");
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseMiddleware<RedirectToDashboardMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

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

app.Run();
