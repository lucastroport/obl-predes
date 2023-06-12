
using AdminServer;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }
    
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseKestrel((hostingContext, options) =>
                {
                    int httpPort = hostingContext.Configuration.GetValue<int>("Server:HttpPort");
                    int httpsPort = hostingContext.Configuration.GetValue<int>("Server:HttpsPort");
                    
                    options.ListenAnyIP(httpsPort, listenOptions =>
                    {
                        listenOptions.UseHttps(); // Enable HTTPS
                    });
                    options.ListenAnyIP(httpPort);
                });
                webBuilder.UseStartup<Startup>();
            })
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
                config.AddEnvironmentVariables();
            });
}