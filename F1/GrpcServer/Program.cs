using GrpcServer.Services;

namespace GrpcServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var server = new global::Server.Server();
        StartServer(server);
        
        builder.Services.AddGrpc();

        var app = builder.Build();
        app.MapGrpcService<PartsService>();
        app.MapGet("/",
            () =>
                "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

        app.Run();
        
    }

    private static async Task StartServer(Server.Server server)
    {
        await Task.Run(server.Start);
    }

}