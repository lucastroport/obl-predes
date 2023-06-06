using LogsServer.Repository;

namespace LogsServer;

public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton<MessageConsumer>();
        services.AddSingleton<ILogsRepository, LogsRepository>();
        services.AddControllers();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}