using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace Repro;

public class MicroService : IMicroService
{
    public bool ExternalLogger = true;

    public MicroService(string name) : this(name, null) { }

    public MicroService(string name, ILogger<IMicroService> logger)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));

        if (logger != null)
        {
            ExternalLogger = true;
            Logger = logger;
        }
    }

    public CancellationTokenSource CancellationTokenSource { get; } = new CancellationTokenSource();
    public IHost Host { get; private set; }    
    public string Name { get; }

    private ILogger<IMicroService> Logger { get; set; }
    public Task InitializeAsync(IConfigurationRoot configuration = null, params string[] args)
    {
        Host = CreateHostBuilder(configuration, args);

        return Task.CompletedTask;
    }
    public async Task RunAsync(IConfigurationRoot configuration = null, params string[] args)
    {
        await InitializeAsync(configuration, args).ConfigureAwait(false);

        try
        {            
            await Host.RunAsync(CancellationTokenSource.Token);
        }
        catch (Exception ex)
        {
            Logger.LogCritical("Unhandled exception in {Service}: {@Exception}", Name, ex);
            throw;
        }
    }
    private IHost CreateHostBuilder(IConfigurationRoot configuration = null, params string[] args)
    {
        var host = global::Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder(args)
            .UseContentRoot(Directory.GetCurrentDirectory())
            .UseConsoleLifetime()
            .ConfigureAppConfiguration((cfg) =>
            {
                if (configuration != null)
                {
                    cfg.AddConfiguration(configuration);
                }
                else
                {
                    cfg
                        .AddJsonFile("appsettings.json", optional: false)                        
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                }
            })
            .ConfigureWebHostDefaults(host =>
            {
                host.Configure(app => 
                {
                    app.UseDeveloperExceptionPage();
                    app.UseSwagger();
                    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "net6ihost v1"));
                    app.UseRouting();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapControllers();
                    });
                });
                
                host.UseSetting(WebHostDefaults.ApplicationKey, Assembly.GetEntryAssembly().FullName);
                
                host.ConfigureServices(services => 
                {
                    services.AddAuthorization();
                    services.AddControllers();
                    services.AddSwaggerGen(c =>
                    {
                        c.SwaggerDoc("v1", new OpenApiInfo { Title = "net6ihost", Version = "v1" });
                    });
                    services.AddMiddlewareAnalysis();
                });
                //app.UseStartup<Startup>();
                // https://github.com/dotnet/aspnetcore/issues/7315#issuecomment-482458078
                //app.UseSetting(WebHostDefaults.ApplicationKey, Assembly.GetEntryAssembly().FullName);
            })
            .Build();        

        return host;
    }
}
