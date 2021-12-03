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

        public async Task CrawlContextWeb()
        {
            var rootWeb = _spClient.Web;
            _spClient.Load(rootWeb);
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
            _spClient.Load(web.Webs);
            await _spClient.ExecuteQueryAsync();

            foreach (var list in web.Lists)
            {

                if (!list.Hidden)
                {
                    await CrawlList(list);
                }
            }
        }

        public async Task<List<SharePointFileVersionInfo>> CrawlList(List list)
        {
            _spClient.Load(list, l=> l.BaseType);
            _spClient.Load(list.RootFolder);
            _spClient.Load(list, l => l.RootFolder.Name);
            await _spClient.ExecuteQueryAsync();

            var results = new List<SharePointFileVersionInfo>();

            var listItems = list.GetItems(new CamlQuery());
            _spClient.Load(listItems);
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

                    _spClient.Load(docListItem.File);
                    _spClient.Load(docListItem.File, i => i.ServerRelativeUrl);
                    await _spClient.ExecuteQueryAsync();

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

            _spClient.Load(item.AttachmentFiles);
            await _spClient.ExecuteQueryAsync();

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
                return new SharePointFileVersionInfo
                {
                    FileRelativePath = url,
                    LastModified = dt,
                    WebUrl = _spClient.Web.Url,
                    SiteUrl = _spClient.Url
                };
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(item), "Can't find modified column");
            }
        }
    }
}
