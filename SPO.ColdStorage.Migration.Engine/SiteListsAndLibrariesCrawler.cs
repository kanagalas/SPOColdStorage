using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Migration.Engine.Model;
using SPO.ColdStorage.Migration.Engine.Utils;

namespace SPO.ColdStorage.Migration.Engine
{
    /// <summary>
    /// Finds files in a SharePoint site collection
    /// </summary>
    public class SiteListsAndLibrariesCrawler
    {
        #region Constructors & Privates

        private readonly ClientContext _spClient;
        private readonly DebugTracer _tracer;
        public event Func<SharePointFileInfo, Task>? _callback;

        public SiteListsAndLibrariesCrawler(ClientContext clientContext, DebugTracer tracer) : this(clientContext, tracer, null)
        {
        }

        public SiteListsAndLibrariesCrawler(ClientContext clientContext, DebugTracer tracer, Func<SharePointFileInfo, Task>? callback)
        {
            this._spClient = clientContext;
            this._tracer = tracer;
            this._callback = callback;
        }

        #endregion

        public async Task CrawlContextRootWebAndSubwebs()
        {
            var rootWeb = _spClient.Web;
            await EnsureContextWebIsLoaded();
            _spClient.Load(rootWeb.Webs);
            await _spClient.ExecuteQueryAsyncWithThrottleRetries();

            await ProcessWeb(rootWeb);

            foreach (var subSweb in rootWeb.Webs)
            {
                await ProcessWeb(subSweb);
            }
        }

        private async Task ProcessWeb(Web web)
        {
            Console.WriteLine($"Reading web '{web.ServerRelativeUrl}'...");
            _spClient.Load(web.Lists);
            await _spClient.ExecuteQueryAsyncWithThrottleRetries();

            foreach (var list in web.Lists)
            {
                _spClient.Load(list, l => l.IsSystemList);
                await _spClient.ExecuteQueryAsyncWithThrottleRetries();

                // Do not search through system or hidden lists
                if (!list.Hidden && !list.IsSystemList && !list.NoCrawl)
                {
                    _tracer.TrackTrace($"Crawling '{list.Title}'...");
                    await CrawlList(list);
                }
                else
                {
                    if (!list.NoCrawl)
                    {
                        _tracer.TrackTrace($"Ignoring '{list.Title}' - NoCrawl set on list.");
                    }
                }
            }
        }

        public async Task<List<SharePointFileInfo>> CrawlList(List list)
        {
            await EnsureContextWebIsLoaded();
            _spClient.Load(list, l => l.BaseType, l => l.ItemCount, l => l.RootFolder);
            await _spClient.ExecuteQueryAsyncWithThrottleRetries();

            var results = new List<SharePointFileInfo>();

            var camlQuery = new CamlQuery();
            camlQuery.ViewXml = "<View Scope=\"RecursiveAll\"><Query>" +
                "<OrderBy><FieldRef Name='ID' Ascending='TRUE'/></OrderBy></Query><RowLimit Paged=\"TRUE\">5000</RowLimit></View>";

            // Large-list support & paging
            ListItemCollection listItems = null!;
            ListItemCollectionPosition currentPosition = null!;
            do
            {
                camlQuery.ListItemCollectionPosition = currentPosition;

                listItems = list.GetItems(camlQuery);
                _spClient.Load(listItems, l => l.ListItemCollectionPosition);

                if (list.BaseType == BaseType.DocumentLibrary)
                {
                    // Load docs
                    _spClient.Load(listItems,
                                     items => items.Include(
                                        item => item.Id,
                                        item => item.FileSystemObjectType,
                                        item => item["Modified"],
                                        item => item["Editor"],
                                        item => item.File.Exists,
                                        item => item.File.ServerRelativeUrl
                                    )
                                );
                }
                else
                {
                    // Generic list, or similar enough. Load attachments
                    _spClient.Load(listItems,
                                     items => items.Include(
                                        item => item.Id,
                                        item => item.AttachmentFiles,
                                        item => item["Modified"],
                                        item => item["Editor"],
                                        item => item.File.Exists,
                                        item => item.File.ServerRelativeUrl
                                    )
                                );
                }

                try
                {
                    await _spClient.ExecuteQueryAsyncWithThrottleRetries();
                }
                catch (System.Net.WebException ex)
                {
                    Console.WriteLine($"Got error reading list: {ex.Message}.");
                }

                // Remember position, if more than 5000 items are in the list
                currentPosition = listItems.ListItemCollectionPosition;
                foreach (var item in listItems)
                {
                    SharePointFileInfo? foundFileInfo = null;
                    if (list.BaseType == BaseType.GenericList)
                    {
                        results.AddRange(await ProcessListItemAttachments(item, list.RootFolder.ServerRelativeUrl));
                    }
                    else if (list.BaseType == BaseType.DocumentLibrary)
                    {
                        foundFileInfo = await ProcessDocLibItem(item, list.RootFolder.ServerRelativeUrl);
                    }
                    if (foundFileInfo != null)
                        results.Add(foundFileInfo!);
                }
            }
            while (currentPosition != null);

            return results;
        }

        /// <summary>
        /// Process document library item.
        /// </summary>
        private async Task<SharePointFileInfo?> ProcessDocLibItem(ListItem docListItem, string listServerRelativeUrl)
        {
            switch (docListItem.FileSystemObjectType)
            {
                case FileSystemObjectType.File:

                    if (docListItem.File.Exists)
                    {
                        var foundFileInfo = GetSharePointFileInfo(docListItem, docListItem.File.ServerRelativeUrl, listServerRelativeUrl);
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
        private async Task<List<SharePointFileInfo>> ProcessListItemAttachments(ListItem item, string listServerRelativeUrl)
        {
            var attachmentsResults = new List<SharePointFileInfo>();

            foreach (var attachment in item.AttachmentFiles)
            {
                var foundFileInfo = GetSharePointFileInfo(item, attachment.ServerRelativeUrl, listServerRelativeUrl);
                if (_callback != null)
                {
                    await this._callback(foundFileInfo);
                }
                attachmentsResults.Add(foundFileInfo);
            }

            return attachmentsResults;
        }


        async Task EnsureContextWebIsLoaded()
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
                await _spClient.ExecuteQueryAsyncWithThrottleRetries();
            }
        }

        SharePointFileInfo GetSharePointFileInfo(ListItem item, string url, string listServerRelativeUrl)
        {
            var dir = "";
            if (item.FieldValues.ContainsKey("FileDirRef"))
            {
                dir = item.FieldValues["FileDirRef"].ToString();
                if (dir!.StartsWith(listServerRelativeUrl))
                {
                    dir = dir.Substring(listServerRelativeUrl.Length);
                }
            }
            else
            {
                throw new ArgumentOutOfRangeException(nameof(item), "Can't find dir column");
            }

            var dt = DateTime.MinValue;
            if (DateTime.TryParse(item.FieldValues["Modified"]?.ToString(), out dt))
            {
                var authorFieldObj = item.FieldValues["Editor"];
                if (authorFieldObj != null)
                {
                    var authorVal = (FieldUserValue)authorFieldObj;
                    return new SharePointFileInfo
                    {
                        Author = !string.IsNullOrEmpty(authorVal.Email) ? authorVal.Email : authorVal.LookupValue,
                        ServerRelativeFilePath = url,
                        LastModified = dt,
                        WebUrl = _spClient.Web.Url,
                        SiteUrl = _spClient.Site.Url,
                        Subfolder = dir.TrimEnd("/".ToCharArray())
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
