using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections;
using System.IO;
using System.Linq;

namespace AP.Search.Web.Nancy
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddCommandLine(args)
                .Build();

            var host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(config)
                .UseKestrel()
                .UseStartup<Startup>()
                .Build();

            AppSettings.Init(config);

            host.Run();
        }
    }
}
