using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Migration.Engine.Model;

namespace SPO.ColdStorage.Migration.Engine
{
    /// <summary>
    /// Finds files in a SharePoint site collection
    /// </summary>
    public class SiteListsAndLibrariesCrawler
    {
        private readonly ClientContext _spClient;
        private readonly DebugTracer _tracer;
        public event Func<SharePointFileVersionInfo, Task>? _callback;

        public SiteListsAndLibrariesCrawler(ClientContext clientContext, DebugTracer tracer) : this(clientContext, tracer, null)
        {
        }

        public SiteListsAndLibrariesCrawler(ClientContext clientContext, DebugTracer tracer, Func<SharePointFileVersionInfo, Task>? callback)
        {
            this._spClient = clientContext;
            this._tracer = tracer;
            this._callback = callback;
        }

        async Task EnsureLoaded()
        {
            var loaded = false;
            try
            {
                // Test if this will blow up
                var url = _spClient.Web.Url;
                url = _spClient.Site.Url;
                loaded = true;
            }
            catch (PropertyOrFieldNotInitializedException)
            {
                loaded = false;
            }

            if (!loaded)
            {
                _spClient.Load(_spClient.Web);
                _spClient.Load(_spClient.Site, s => s.Url);
                await _spClient.ExecuteQueryAsync();
            }
        }


        public async Task CrawlContextWeb()
        {
            var rootWeb = _spClient.Web;
            await EnsureLoaded();
            _spClient.Load(rootWeb.Webs);
            await _spClient.ExecuteQueryAsync();

            await ProcessWeb(rootWeb);

            foreach (var subSweb in rootWeb.Webs)
            {
                await ProcessWeb(subSweb);
            }
        }

        private async Task ProcessWeb(Web web)
        {
            _spClient.Load(web.Lists);
            await _spClient.ExecuteQueryAsync();

            foreach (var list in web.Lists)
            {
                _spClient.Load(list, l => l.IsSystemList);
                await _spClient.ExecuteQueryAsync();

                if (!list.Hidden && !list.IsSystemList)
                {
                    await CrawlList(list);
                }
            }
        }

        public async Task<List<SharePointFileVersionInfo>> CrawlList(List list)
        {
            await EnsureLoaded();
            _spClient.Load(list, l => l.BaseType);
            await _spClient.ExecuteQueryAsync();

            var results = new List<SharePointFileVersionInfo>();

            var camlQuery = new CamlQuery();
            var listItems = list.GetItems(camlQuery);

            if (list.BaseType == BaseType.GenericList)
            {
                // Load attachments
                _spClient.Load(listItems,
                                 items => items.Include(
                                    item => item.Id,
                                    item => item.AttachmentFiles,
                                    item => item["Modified"],
                                    item => item["Editor"],
                                    item => item.File.Exists,
                                    item => item.File.ServerRelativeUrl));
            }
            else if (list.BaseType == BaseType.DocumentLibrary)
            {
                // Load docs
                _spClient.Load(listItems,
                                 items => items.Include(
                                    item => item.Id,
                                    item => item.FileSystemObjectType,
                                    item => item["Modified"],
                                    item => item["Editor"],
                                    item => item.File.Exists,
                                    item => item.File.ServerRelativeUrl));
            }

            await _spClient.ExecuteQueryAsync();

            foreach (var item in listItems)
            {
                SharePointFileVersionInfo? foundFileInfo = null;
                if (list.BaseType == BaseType.GenericList)
                {
                    results.AddRange(await ProcessListItemAttachments(item));
                }
                else if (list.BaseType == BaseType.DocumentLibrary)
                {
                    foundFileInfo = await ProcessDocLibItem(item);
                }
                if (foundFileInfo != null)
                    results.Add(foundFileInfo!);
            }

            return results;
        }

        /// <summary>
        /// Process document library item.
        /// </summary>
        private async Task<SharePointFileVersionInfo?> ProcessDocLibItem(ListItem docListItem)
        {
            switch (docListItem.FileSystemObjectType)
            {
                case FileSystemObjectType.File:

                    if (docListItem.File.Exists)
                    {
                        var foundFileInfo = GetSharePointFileInfo(docListItem, docListItem.File.ServerRelativeUrl);
                        if (_callback != null)
                        {
                            await this._callback(foundFileInfo);
                        }

                        return foundFileInfo;
                    }
                    break;
            }

            return null;
        }

        /// <summary>
        /// Process custom list item attachments
        /// </summary>
        private async Task<List<SharePointFileVersionInfo>> ProcessListItemAttachments(ListItem item)
        {
            var attachmentsResults = new List<SharePointFileVersionInfo>();

            foreach (var attachment in item.AttachmentFiles)
            {
                var foundFileInfo = GetSharePointFileInfo(item, attachment.ServerRelativeUrl);
                if (_callback != null)
                {
                    await this._callback(foundFileInfo);
                }
                attachmentsResults.Add(foundFileInfo);
            }

            return attachmentsResults;
        }


        SharePointFileVersionInfo GetSharePointFileInfo(ListItem item, string url)
        {
            var dt = DateTime.MinValue;
            if (DateTime.TryParse(item.FieldValues["Modified"]?.ToString(), out dt))
            {
                var authorFieldObj = item.FieldValues["Editor"];
                if (authorFieldObj != null)
                {
                    var authorVal = (FieldUserValue)authorFieldObj;
                    return new SharePointFileVersionInfo
                    {
                        Author = !string.IsNullOrEmpty(authorVal.Email) ? authorVal.Email : authorVal.LookupValue,
                        FileRelativePath = url,
                        LastModified = dt,
                        WebUrl = _spClient.Web.Url,
                        SiteUrl = _spClient.Site.Url
                    };
                }
                else
                {
                    throw new ArgumentOutOfRangeException(nameof(item), "Can't find author column");
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(item), "Can't find modified column");
            }
        }
    }
}
