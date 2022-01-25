using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DataService;
using FunctionalService;
using LoggingService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PokerAPI.Hubs;
using Serilog;
using Serilog.Sinks.SystemConsole.Themes;

namespace PokerAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            //CreateHostBuilder(args).Build().Run();
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var dbContext = services.GetRequiredService<ApplicationDbContext>();
                    var dpkContext = services.GetRequiredService<DataProtectionKeysContext>();
                    var functionalSvc = services.GetRequiredService<IFunctionalSvc>();
                    //var hubContext = host.Services.GetService(typeof(IHubContext<RoomHub>));

                    DbContextInitializer.Initialize(dpkContext, dbContext, functionalSvc).Wait();
                }
                catch (Exception ex)
                {
                    Log.Error("Error occured on startup in Program Class with data: {Error} {StackTrace} {InnerException} {Source}",
                        ex.Message, ex.StackTrace, ex.InnerException, ex.Source);
                }
            }

            host.Run();
        }
        //test comment
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                        .Enrich.FromLogContext()
                        .Enrich.WithProperty("Application", "Poker_API")
                        .Enrich.WithProperty("MachineName", Environment.MachineName)
                        .Enrich.WithProperty("CurrentManagedThreadId", Environment.CurrentManagedThreadId)
                        .Enrich.WithProperty("OSVersion", Environment.OSVersion)
                        .Enrich.WithProperty("Version", Environment.Version)
                        .Enrich.WithProperty("UserName", Environment.UserName)
                        .Enrich.WithProperty("ProcessId", Process.GetCurrentProcess().Id)
                        .Enrich.WithProperty("ProcessName", Process.GetCurrentProcess().ProcessName)
                        .WriteTo.Console(theme: AnsiConsoleTheme.None)
                        .WriteTo.File(formatter: new CustomTextFormatter(),
                                      path: Path.Combine(hostingContext.HostingEnvironment.ContentRootPath +
                                      $"{Path.DirectorySeparatorChar}Logs{Path.DirectorySeparatorChar}",
                                      $"load_error_{DateTime.Now:yyyyMMdd}.txt"))
                        .ReadFrom.Configuration(hostingContext.Configuration));
                    webBuilder.UseStartup<Startup>()
                    .UseIISIntegration();
                });
    }
}