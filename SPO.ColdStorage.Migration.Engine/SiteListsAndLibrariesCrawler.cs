using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Migration.Engine.Model;

namespace SPO.ColdStorage.Migration.Engine
{
    /// <summary>
    /// Finds files in a SharePoint site collection
    /// </summary>
    internal class SiteListsAndLibrariesCrawler
    {
        private readonly ClientContext _spClient;
        private readonly DebugTracer _tracer;
        public event EventHandler<SharePointFileInfoEventArgs>? SharePointFileFound;

        public SiteListsAndLibrariesCrawler(ClientContext clientContext, DebugTracer tracer)
        {
            this._spClient = clientContext;
            this._tracer = tracer;
        }

        internal async Task Start()
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
            _tracer.TrackTrace($"Reading site {web.Url}");

            _spClient.Load(web.Lists);
            _spClient.Load(web.Webs);
            await _spClient.ExecuteQueryAsync();

            foreach (var list in web.Lists)
            {
                _spClient.Load(list.RootFolder);
                _spClient.Load(list, l => l.RootFolder.Name);
                await _spClient.ExecuteQueryAsync();

                if (!list.Hidden)
                {
                    await ProcessListItemsRequest(list);
                }
                else
                {
                    _tracer.TrackTrace($"Ignoring hidden list {list.RootFolder.ServerRelativeUrl}");
                }
            }
        }

        private async Task ProcessListItemsRequest(List list)
        {
            var listItems = list.GetItems(new CamlQuery());
            _spClient.Load(listItems);
            await _spClient.ExecuteQueryAsync();

            _tracer.TrackTrace($"List URL: {list.RootFolder.ServerRelativeUrl}");

            foreach (var item in listItems)
            {
                if (list.BaseType == BaseType.GenericList)
                {
                    await ProcessListItem(item);
                }
                else if (list.BaseType == BaseType.DocumentLibrary)
                {
                    await ProcessDocLibItem(item);
                }
            }
        }

        private async Task ProcessDocLibItem(ListItem item)
        {
            switch (item.FileSystemObjectType)
            {
                case FileSystemObjectType.File:

                    _spClient.Load(item.File);
                    _spClient.Load(item.File, i => i.ServerRelativeUrl);
                    await _spClient.ExecuteQueryAsync();

                    if (item.File.Exists)
                    {
                        this.SharePointFileFound?.Invoke(this, new SharePointFileInfoEventArgs
                        {
                            SharePointFileInfo = GetSharePointFileInfo(item, item.File.ServerRelativeUrl)
                        });
                    }
                    break;
            }
        }

        private async Task ProcessListItem(ListItem item)
        {
            _spClient.Load(item.AttachmentFiles);
            await _spClient.ExecuteQueryAsync();

            foreach (var attachment in item.AttachmentFiles)
            {
                this.SharePointFileFound?.Invoke(this, new SharePointFileInfoEventArgs
                {
                    SharePointFileInfo = GetSharePointFileInfo(item, attachment.ServerRelativeUrl)
                });
            }
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
