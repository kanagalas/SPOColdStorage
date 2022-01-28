using SPO.ColdStorage.Entities.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine.Model
{
    public class TargetMigrationSiteDTO : TargetMigrationSite
    {
        private SiteListFilterConfig? _siteListFilterConfig;
        public TargetMigrationSiteDTO()
        {
        }
        public TargetMigrationSiteDTO(TargetMigrationSite target)
        {
            this.RootURL = target.RootURL;
            this.FilterConfigJson = target.FilterConfigJson;
        }

        public SiteListFilterConfig SiteFilterConfig 
        { 
            get 
            {
                if (_siteListFilterConfig == null)
                {
                    if (!string.IsNullOrEmpty(FilterConfigJson))
                    {
                        try
                        {
                            _siteListFilterConfig = System.Text.Json.JsonSerializer.Deserialize<SiteListFilterConfig>(FilterConfigJson);
                        }
                        catch (Exception)
                        {
                            // Ignore
                        }
                    }
                    if (_siteListFilterConfig == null)
                    {
                        // Something went wrong. Create allow-all vconfig
                        _siteListFilterConfig = new SiteListFilterConfig();
                    }
                }
                return _siteListFilterConfig; 
            } 
        }
    }
}
