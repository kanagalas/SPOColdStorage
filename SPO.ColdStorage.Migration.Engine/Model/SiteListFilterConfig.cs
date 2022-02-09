using SPO.ColdStorage.Entities.DBEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine.Model
{
    /// <summary>
    /// Configuration for what to filter for a site. 
    /// </summary>
    public class SiteListFilterConfig
    {
        /// <summary>
        /// Lists to filter on. If empty will allow all lists
        /// </summary>
        public List<ListFolderConfig> ListFilterConfig { get; set; } = new List<ListFolderConfig>();


        public ListFolderConfig GetListFolderConfig(string listTitle)
        {
            if (ListFilterConfig.Count == 0)
            {
                return new ListFolderConfig();
            }
            else
            {
                var listConfig = FindListFolderConfig(listTitle);
                if (listConfig != null) 
                    return listConfig;
                else
                    return new ListFolderConfig();      // Allow all
            }
        }

        ListFolderConfig? FindListFolderConfig(string listTitle)
        {
            return ListFilterConfig.Where(l => l.ListTitle == listTitle).FirstOrDefault();
        }

        public bool IncludeListInMigration(string listTitle)
        {
            if (ListFilterConfig.Count == 0)
            {
                return true;
            }
            else
            {
                var listFolderConfig = FindListFolderConfig(listTitle);
                return listFolderConfig != null;
            }
        }

        public bool IncludeFolderInMigration(string listTitle, string folderUrl)
        {
            // Allow root
            if (folderUrl == string.Empty)
            {
                return true;
            }

            // No config set - allow all
            if (ListFilterConfig.Count == 0)
            {
                return true;
            }
            else
            {
                var listFolderConfig = FindListFolderConfig(listTitle);
                if (listFolderConfig == null)
                {
                    return false;
                }
                else
                {
                    return listFolderConfig.IncludeFolderInMigration(folderUrl);
                }
            }
        }

        public string ToJson()
        {
            return System.Text.Json.JsonSerializer.Serialize(this);
        }
        public static SiteListFilterConfig FromJson(string filterConfigJson)
        {
            if (string.IsNullOrEmpty(filterConfigJson))
            {
                throw new ArgumentException($"'{nameof(filterConfigJson)}' cannot be null or empty.", nameof(filterConfigJson));
            }

            return System.Text.Json.JsonSerializer.Deserialize<SiteListFilterConfig>(filterConfigJson)!;
        }
    }

    /// <summary>
    /// Folder whitelist for a list
    /// </summary>
    public class ListFolderConfig
    {
        public string ListTitle { get; set; } = string.Empty;

        public List<string> FolderWhiteList { get; set; } = new List<string>();
        public bool IncludeFolderInMigration(string url)
        {
            if (url == String.Empty)
            {
                return true;
            }
            if (FolderWhiteList.Count == 0)
            {
                return true;
            }
            else
            {
                return FolderWhiteList.Where(f=> f.ToLower() == url.ToLower()).Any();
            }
        }

        internal bool IncludeFolder(SharePointFileInfo fileInfo)
        {
            return IncludeFolderInMigration(fileInfo.Subfolder);
        }
    }
}
