using GuidesApi.Data;
using GuidesApi.Extensions;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace MedSelectApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateWebHostBuilder(args).Build().Run();


            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, true)
                .AddEnvironmentVariables()
                .Build();

            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var dbContext = services.GetRequiredService<AppDbContext>();

                var configuration = services.GetRequiredService<IConfiguration>();
                var accessor = services.GetRequiredService<IHttpContextAccessor>();

                Log.Logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(config)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.DapperSink(configuration, accessor)
                    .CreateLogger();

                try
                {
                    dbContext.Database.SetCommandTimeout(100);
                    dbContext.Database.Migrate();
                    host.Run();
                }
                catch (Exception ex)
                {
                    Log.Fatal(ex, "Host terminated unexpectedly");
                }
                finally
                {
                    dbContext.Database.SetCommandTimeout(30);
                    Log.CloseAndFlush();
                    Thread.Sleep(1000);
                }
            }

        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseLamar()
                .UseSerilog()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.ConfigureServices(services =>
                    {
                        // This is important, the call to AddControllers()
                        // cannot be made before the usage of ConfigureWebHostDefaults
                        services.AddControllers();
                    });
                });
    }
}