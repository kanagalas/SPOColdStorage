using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Tests
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World! This is a console app for testing whatever isn't working. Shouldn't be run normally.");

            var config = ConsoleUtils.GetConfigurationWithDefaultBuilder();

            var host = new HostBuilder()
                .ConfigureAppConfiguration(c =>
                {
                    c.AddConfiguration(ConsoleUtils.GetConfigurationBuilder().Build());
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddDbContext<SPOColdStorageDbContext>(options => options
                        .UseSqlServer(config.ConnectionStrings.SQLConnectionString,
                        moreOptions => moreOptions.CommandTimeout(120))
                    );

                })
                .Build();

            host.Run();



            var optionsBuilder = new DbContextOptionsBuilder<SPOColdStorageDbContext>();
            optionsBuilder
                .UseSqlServer(config.ConnectionStrings.SQLConnectionString);

            
        }
    }
}
