using Microsoft.Extensions.Configuration;
using SPO.ColdStorage.Entities;
using System.Reflection;

namespace SPO.ColdStorage.Migration.Engine.Utils
{
    public class ConsoleUtils
    {
        public static Config GetConfigurationBuilder()
        {

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddUserSecrets(System.Reflection.Assembly.GetEntryAssembly())
                .AddEnvironmentVariables()
                .AddJsonFile("appsettings.json", true);

            var configCollection = builder.Build();
            return new Config(configCollection);
        }
    }
}
