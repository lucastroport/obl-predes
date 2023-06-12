using LogsServer.Repository;

namespace LogsServer;

public class Program
{
    static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ILogsRepository, LogsRepository>();
                services.AddSingleton<MessageConsumer>();
            })
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel((hostingContext, options) =>
                {
                    // Get the port from appsettings.json
                    int httpPort = hostingContext.Configuration.GetValue<int>("Server:HttpPort");
                    int httpsPort = hostingContext.Configuration.GetValue<int>("Server:HttpsPort");
                    options.ListenAnyIP(httpPort);
                    options.ListenAnyIP(httpsPort, listenOptions =>
                    {
                        listenOptions.UseHttps(); // Enable HTTPS
                    });
                });
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            })
            .Build();
        
        var messageConsumer = host.Services.GetRequiredService<MessageConsumer>();

        var webTask = host.RunAsync();

        var queueTask = Task.Run(() => messageConsumer.StartConsuming());

        await Task.WhenAll(webTask, queueTask);
    }
}