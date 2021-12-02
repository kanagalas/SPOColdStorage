using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Entities.Configuration
{
    public class DevConfig : BaseConfig
    {
        public DevConfig(Microsoft.Extensions.Configuration.IConfigurationSection config) : base(config)
        {
        }

        public string DefaultStorageConnection { get; set; } = string.Empty;
        public string DefaultSharePointSite { get; set; } = string.Empty;
    }
}
