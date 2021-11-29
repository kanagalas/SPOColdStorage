using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Entities
{
    public class DevConfig
    {
        public DevConfig()
        {
        }

        public DevConfig(Microsoft.Extensions.Configuration.IConfigurationSection config)
        {
            this.DefaultStorageConnection = config["DefaultStorageConnection"];
            this.DefaultSharePointSite = config["DefaultSharePointSite"];
        }

        public string DefaultStorageConnection { get; set; } = string.Empty;
        public string DefaultSharePointSite { get; set; } = string.Empty;
    }
}
