using SPO.ColdStorage.Models;

namespace SPO.ColdStorage.Migration.Engine
{
    /// <summary>
    /// Finds files in a SharePoint site collection
    /// </summary>
    public class SiteListsAndLibrariesCrawler<T>
    {
        #region Constructors & Privates

        private readonly DebugTracer _tracer;
        private readonly ISiteCollectionLoader<T> _crawlConnector;

        public SiteListsAndLibrariesCrawler(ISiteCollectionLoader<T> crawlConnector, DebugTracer tracer)
        {
            _crawlConnector = crawlConnector;
            this._tracer = tracer;
        }

        #endregion

        public async Task StartSiteCrawl(SiteListFilterConfig siteFolderConfig, Func<SharePointFileInfoWithList, Task>? foundFileCallback, Action? crawlComplete)
        {
            var webs = await _crawlConnector.GetWebs();

            foreach (var subSweb in webs)
            {
                await ProcessWeb(subSweb, siteFolderConfig, foundFileCallback);
            }
            crawlComplete?.Invoke();
        }

        private async Task ProcessWeb(IWebLoader<T> web, SiteListFilterConfig siteFolderConfig, Func<SharePointFileInfoWithList, Task>? foundFileCallback)
        {
            var lists = await web.GetLists();

            foreach (var list in lists)
            {
                if (siteFolderConfig.IncludeListInMigration(list.Title))
                {
                    var listConfig = siteFolderConfig.GetListFolderConfig(list.Title);
                    await CrawlList(list, listConfig, foundFileCallback);
                }
                else
                {
                    _tracer.TrackTrace($"Skipping list '{list.Title}'");
                }
            }
        }
        public async Task<SiteCrawlContentsAndStats> CrawlList(IListLoader<T> parentList, ListFolderConfig listConfig, Func<SharePointFileInfoWithList, Task>? foundFileCallback)
        {
            PageResponse<T>? listPage = null;

            var listResults = new SiteCrawlContentsAndStats();
            T? token = default(T);

            int pageCount = 1;
            while (listPage == null || listPage.NextPageToken != null)
            {
                listPage = await parentList.GetListItems(token);
                token = listPage.NextPageToken;

                // Filter files
                foreach (var file in listPage.FilesFound)
                {
                    if (listConfig.IncludeFolder(file))
                    {
                        if (foundFileCallback != null)
                        {
                            await foundFileCallback.Invoke(file);
                        }
                        listResults.FilesFound.Add(file);
                    }
                    else
                    {
                        listResults.IgnoredFiles++;
                    }
                }
                _tracer.TrackTrace($"Loaded {listPage.FilesFound.Count.ToString("N0")} files and {listPage.FoldersFound.Count.ToString("N0")} folders from list '{parentList.Title}' on page {pageCount}");

                // Add unique folders
                listResults.FoldersFound.AddRange(listPage.FoldersFound.Where(newFolderFound => !listResults.FoldersFound.Contains(newFolderFound)));

                pageCount++;
            }
            
            return listResults;

        }
    }
}
