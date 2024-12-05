using ASI.Basecode.Data;
using ASI.Basecode.WebApp;
using ASI.Basecode.WebApp.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;

var appBuilder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
});

appBuilder.Services.AddDbContext<AssisthubDBContext>(options =>
{
    var connectionString = appBuilder.Configuration.GetConnectionString("RemoteConnection");
    options.UseSqlServer(connectionString);
});

appBuilder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option =>
    {
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

appBuilder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
appBuilder.WebHost.UseIISIntegration();

appBuilder.Logging
    .AddConfiguration(appBuilder.Configuration.GetLoggingSection())
    .AddConsole()
    .AddDebug();

var configurer = new StartupConfigurer(appBuilder.Configuration);
configurer.ConfigureServices(appBuilder.Services);

var app = appBuilder.Build();

// Global exception handler for non-development environments
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

// Handle custom HTTP status codes like 404 and 403
app.Use(async (context, next) =>
{
    await next();
    if (context.Response.StatusCode == 404)
    {
        context.Request.Path = "/Error/Error404";
        await next();
    }
    else if (context.Response.StatusCode == 403)
    {
        context.Request.Path = "/Error/Error403";
        await next();
    }
});

configurer.ConfigureApp(app, app.Environment);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}");
app.MapControllers();
app.MapRazorPages();

app.Run();
