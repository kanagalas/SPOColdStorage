using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine
{
    public class AzureAdConfig
    {
        public AzureAdConfig(Microsoft.Extensions.Configuration.IConfigurationRoot config)
        {
            var aadConfig = config.GetSection("AzureAd");
            this.ClientID = aadConfig["ClientID"];
            this.Secret = aadConfig["Secret"];
            this.TenantId = aadConfig["TenantId"];
        }

        public string Secret { get; set; }
        public string ClientID { get; set; }
        public string TenantId { get; set; }
    }
}
