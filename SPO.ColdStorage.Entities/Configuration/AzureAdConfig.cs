using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Entities.Configuration
{
    public class AzureAdConfig : BaseConfig
    {
        public AzureAdConfig(Microsoft.Extensions.Configuration.IConfigurationSection config) : base(config)
        {
        }

        public string? Secret { get; set; }
        public string? ClientID { get; set; }
        public string? TenantId { get; set; }
    }
}
