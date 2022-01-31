using SPO.ColdStorage.Entities.DBEntities;
using System.Text.Json.Serialization;

namespace SPO.ColdStorage.Migration.Engine.Model
{
    /// <summary>
    /// TargetMigrationSite + model from Json
    /// </summary>
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

        [JsonIgnore]
        public bool IsValid => !Uri.IsWellFormedUriString(RootURL, UriKind.Absolute);

        /// <summary>
        /// Model version of Json
        /// </summary>
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
            set
            {
                this.FilterConfigJson = System.Text.Json.JsonSerializer.Serialize(this);
            }
        }
    }
}
