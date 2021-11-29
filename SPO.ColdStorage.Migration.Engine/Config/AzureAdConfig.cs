using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine
{
    public class AzureAdConfig
    {
        public AzureAdConfig(Microsoft.Extensions.Configuration.IConfigurationSection config)
        {
            this.ClientID = config["ClientID"];
            this.Secret = config["Secret"];
            this.TenantId = config["TenantId"];
        }

        public string Secret { get; set; }
        public string ClientID { get; set; }
        public string TenantId { get; set; }
    }
}
