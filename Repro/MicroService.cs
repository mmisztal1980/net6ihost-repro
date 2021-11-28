using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
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
            .ConfigureWebHostDefaults(app => 
            {
                app.UseStartup<Startup>();
            })            
            .Build();        

        return host;
    }
}