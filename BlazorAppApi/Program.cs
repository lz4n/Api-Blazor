using BlazorAppApi.ApiUtils;
using BlazorAppApi.AppState;
using BlazorAppApi.Components;
using BlazorBootstrap;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Serilog;
using Serilog.Exceptions;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using BlazorAppApi.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

SetupSerilog(builder);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddHealthChecks()
    .AddCheck<ApiHealthCheck>("CarAPI");

builder.Services.AddSingleton<AppState>();

builder.Services.AddBlazorBootstrap();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapHealthChecks("/hc", new HealthCheckOptions()
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse

});   

app.MapHealthChecks("/liveness"); ;

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();

static Serilog.ILogger CreateSerilogLogger(ConfigurationManager configuration)
{
    var seqServerUrl = configuration["Serilog:SeqServerUrl"];

    return new LoggerConfiguration()
        .MinimumLevel.Verbose()
        .Enrich.WithExceptionDetails()
        .Enrich.WithProperty("ApplicationContext", typeof(Program).GetTypeInfo().Assembly.GetName().Name)
        .Enrich.FromLogContext()

        .WriteTo.Console()
        .WriteTo.Seq(string.IsNullOrWhiteSpace(seqServerUrl) ? "http://seq" : seqServerUrl)

        .ReadFrom.Configuration(configuration)

        .CreateLogger();
}

static void SetupSerilog(WebApplicationBuilder builder)
{
    IWebHostEnvironment env = builder.Environment;
    builder.Configuration
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: false)
        .AddEnvironmentVariables()
        .Build();

    builder.Host.UseSerilog();
    Log.Logger = CreateSerilogLogger(builder.Configuration);

    Log.Information("Starting web host ({ApplicationContext})",
        typeof(Program).GetTypeInfo().Assembly.GetName().Name);
}