using System;
using System.IO;
using ASI.Basecode.Data;
using ASI.Basecode.WebApp;
using ASI.Basecode.WebApp.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using System.Reflection.PortableExecutable;

var appBuilder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = Directory.GetCurrentDirectory(),
});

appBuilder.Services.AddDbContext<TicketingSystemDBContext>(options =>
{
    var connectionString = appBuilder.Configuration.GetConnectionString("Defau");
    options.UseSqlServer(connectionString);
});

appBuilder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme
    ).AddCookie(option =>
    {
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

appBuilder.Configuration.AddJsonFile("appsettings.json",
    optional: true,
    reloadOnChange: true);

appBuilder.WebHost.UseIISIntegration();

appBuilder.Logging
    .AddConfiguration(appBuilder.Configuration.GetLoggingSection())
    .AddConsole()
    .AddDebug();

var configurer = new StartupConfigurer(appBuilder.Configuration);
configurer.ConfigureServices(appBuilder.Services);

var app = appBuilder.Build();

//app.Use(async (context, next) =>
//{
//    await next();
//    if (context.Response.StatusCode == 404)
//    {
//        context.Request.Path = "/Error/error404";
//        await next();
//    }
//    else if (context.Response.StatusCode == 403)
//    {
//        context.Request.Path = "/Error/error403";
//        await next();
//    }
//});

configurer.ConfigureApp(app, app.Environment);

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}");
app.MapControllers();
app.MapRazorPages();

// Run application
app.Run();


//var builder = WebApplication.CreateBuilder(new WebApplicationOptions
//{
//    ContentRootPath = Directory.GetCurrentDirectory(),
//});


//builder.Services.AddDbContext<TicketingSystemDBContext>(options =>
//{
//    var connectionString = "Data Source=.\\sqlexpress;Initial Catalog=TicketingSystemDB;Integrated Security=True;Encrypt=True;Trust Server Certificate=True";
//    options.UseSqlServer(connectionString);
//});


//builder.Services.AddAuthentication(
//    CookieAuthenticationDefaults.AuthenticationScheme
//    ).AddCookie(option =>
//    {
//        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
//    });

//builder.Services.AddTransient<IAuthorizationHandler, RolesInDBAuthorizationHandler>();

//builder.Services.AddAuthorization(options =>
//{
//    options.AddPolicy("SuperAdminPolicy", policy =>
//        policy.RequireRole("superadmin"));

//    options.AddPolicy("AdminPolicy", policy =>
//        policy.RequireRole("administrator"));

//    options.AddPolicy("SupportAgentPolicy", policy =>
//        policy.RequireRole("support agent"));

//    options.AddPolicy("UserPolicy", policy =>
//        policy.RequireRole("user"));
//});

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//builder.Configuration.AddJsonFile("appsettings.json",
//    optional: true,
//    reloadOnChange: true);

//builder.WebHost.UseIISIntegration();

//builder.Logging
//    .AddConfiguration(builder.Configuration.GetLoggingSection())
//    .AddConsole()
//    .AddDebug();

//var configurer = new StartupConfigurer(builder.Configuration);
//configurer.ConfigureServices(builder.Services);

//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}
//app.Use(async (context, next) =>
//{
//    await next();
//    if (context.Response.StatusCode == 404)
//    {
//        context.Request.Path = "/Error/error404";
//        await next();
//    }
//    else if (context.Response.StatusCode == 403)
//    {
//        context.Request.Path = "/Error/error403";
//        await next();
//    }
//});

//configurer.ConfigureApp(app, app.Environment);

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();
//app.UseAuthentication();
//app.UseAuthorization();

//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Account}/{action=LogIn}/{id?}");

//app.MapControllers();
//app.MapRazorPages();

//app.Run();