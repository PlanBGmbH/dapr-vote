using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace Notifications
{
    /// <summary>
    /// The program to execute.
    /// </summary>
    public static class Program
    {
        private const int Port = 3002;

        /// <summary>
        /// Main entry point for the program.
        /// </summary>
        /// <param name="args">The program arguments.</param>
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        // Additional configuration is required to successfully run gRPC on macOS.
        // For instructions on how to configure Kestrel and gRPC clients on macOS,
        // visit https://go.microsoft.com/fwlink/?linkid=2099682
        private static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseUrls($"http://localhost:{Port}/");
                });
    }
}
