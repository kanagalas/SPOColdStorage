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
        public event EventHandler<SharePointFileInfoEventArgs>? SharePointFileFound;

        public SiteListsAndLibrariesCrawler(ClientContext clientContext, DebugTracer tracer)
        {
            this._spClient = clientContext;
            this._tracer = tracer;
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

        public async Task<List<SharePointFileUpdateInfo>> CrawlList(List list)
        {
            _spClient.Load(list, l=> l.BaseType);
            _spClient.Load(list.RootFolder);
            _spClient.Load(list, l => l.RootFolder.Name);
            await _spClient.ExecuteQueryAsync();

            var results = new List<SharePointFileUpdateInfo>();

            var listItems = list.GetItems(new CamlQuery());
            _spClient.Load(listItems);
            await _spClient.ExecuteQueryAsync();


            foreach (var item in listItems)
            {
                SharePointFileUpdateInfo? foundFileInfo = null;
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
        private async Task<SharePointFileUpdateInfo?> ProcessDocLibItem(ListItem docListItem)
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
                        var args = new SharePointFileInfoEventArgs
                        {
                            SharePointFileInfo = foundFileInfo
                        };
                        this.SharePointFileFound?.Invoke(this, args);

                        return foundFileInfo;
                    }
                    break;
            }

            return null;
        }

        /// <summary>
        /// Process custom list item attachments
        /// </summary>
        private async Task<List<SharePointFileUpdateInfo>> ProcessListItemAttachments(ListItem item)
        {
            var attachmentsResults = new List<SharePointFileUpdateInfo>();

            _spClient.Load(item.AttachmentFiles);
            await _spClient.ExecuteQueryAsync();

            foreach (var attachment in item.AttachmentFiles)
            {
                var foundFileInfo = GetSharePointFileInfo(item, attachment.ServerRelativeUrl);
                var args = new SharePointFileInfoEventArgs
                {
                    SharePointFileInfo = foundFileInfo
                };
                this.SharePointFileFound?.Invoke(this, args);
                attachmentsResults.Add(foundFileInfo);
            }

            return attachmentsResults;
        }


        SharePointFileUpdateInfo GetSharePointFileInfo(ListItem item, string url)
        {
            var dt = DateTime.MinValue;
            if (DateTime.TryParse(item.FieldValues["Modified"]?.ToString(), out dt))
            {
                return new SharePointFileUpdateInfo
                {
                    FileRelativePath = url,
                    LastModified = dt,
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
