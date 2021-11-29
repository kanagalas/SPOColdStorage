using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Entities
{
    public class Config
    {
        public Config(Microsoft.Extensions.Configuration.IConfigurationRoot config)
        {
            if (config is null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            var aadConfig = config.GetSection("AzureAd");
            if (aadConfig != null)
                this.AzureAdConfig = new AzureAdConfig(aadConfig);
            else
                throw new ConfigException("Missing Azure AD configuration");

            var devConfig = config.GetSection("Dev");
            if (devConfig != null)
                this.DevConfig = new DevConfig(devConfig);
            else
                this.DevConfig = new DevConfig();

            var connectionStringsConfig = config.GetSection("ConnectionStrings");

            this.SQLConnectionString = connectionStringsConfig["SQLConnectionString"];
            this.ServiceBusConnectionString = connectionStringsConfig["ServiceBus"];
        }
        public string SQLConnectionString { get; set; }
        public string ServiceBusConnectionString { get; set; }

        public AzureAdConfig AzureAdConfig { get; set; }
        public DevConfig DevConfig { get; set; }
    }

    public class ConfigException : Exception
    {
        public ConfigException(string message) : base(message) { }
    }
}
