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
                webBuilder.UseKestrel();
                webBuilder.UseStartup<Startup>();
            })
            .Build();
        
        var messageConsumer = host.Services.GetRequiredService<MessageConsumer>();

        var webTask = host.RunAsync();

        var queueTask = Task.Run(() => messageConsumer.StartConsuming());

        await Task.WhenAll(webTask, queueTask);
    }
}