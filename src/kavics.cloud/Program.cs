using Serilog;

namespace kavics.cloud;

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
                webBuilder.UseStartup<Startup>()
                    .ConfigureLogging(loggingConfiguration =>
                        loggingConfiguration.ClearProviders());
            })
            .UseSerilog((hostingContext, loggerConfiguration) =>
                loggerConfiguration.ReadFrom
                    .Configuration(hostingContext.Configuration));
}