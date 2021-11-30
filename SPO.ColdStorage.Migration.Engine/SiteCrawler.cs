using Microsoft.SharePoint.Client;
using SPO.ColdStorage.Entities;

namespace SPO.ColdStorage.Migration.Engine
{
    internal class SiteCrawler
    {
        private readonly ClientContext _graphServiceClient;
        private readonly string _siteId;
        private readonly ColdStorageDbContext _db;
        private readonly DebugTracer _tracer;

        public SiteCrawler(ClientContext graphServiceClient, string siteId, ColdStorageDbContext db, DebugTracer tracer)
        {
            this._graphServiceClient = graphServiceClient;
            this._siteId = siteId;
            this._db = db;
            this._tracer = tracer;
        }

        internal async Task Start()
        {

            var web = _graphServiceClient.Web;
            _graphServiceClient.Load(web);
            await _graphServiceClient.ExecuteQueryAsync();

            //var rootSite = await _graphServiceClient.Sites[_siteId].Request().GetAsync();
            //await ProcessSite(rootSite);

            //// Get subwebs
            //await ProcessSubwebsRequest(_graphServiceClient.Sites[_siteId].Sites.Request());

        }

        //private async Task ProcessSubwebsRequest(ISiteSitesCollectionRequest sitesCollectionRequest)
        //{
        //    var allSites = await sitesCollectionRequest.GetAsync();
        //    if (allSites.NextPageRequest != null)
        //    {
        //        await ProcessSubwebsRequest(allSites.NextPageRequest);
        //    }

        //    foreach (var site in allSites)
        //    {
        //        await ProcessSite(site);
        //    }
        //}

        //private async Task ProcessSite(Site site)
        //{
        //    _tracer.TrackTrace($"Reading site {site.WebUrl}");
        //    var siteLists = await _graphServiceClient.Sites[_siteId].Sites[site.Id].Lists.Request().GetAsync();
        //    foreach (var list in siteLists)
        //    {
        //        await ProcessListItemsRequest(_graphServiceClient.Sites[_siteId].Sites[site.Id].Lists[list.Id].Items.Request().Expand("fields"), list);
        //    }
        //}

        //private async Task ProcessListItemsRequest(IListItemsCollectionRequest listItemsCollectionRequest, List list)
        //{
        //    _tracer.TrackTrace($"--List {list.WebUrl}");

        //    var listItems = await listItemsCollectionRequest.GetAsync();
        //    if (listItems.NextPageRequest != null)
        //    {
        //        await ProcessListItemsRequest(listItems.NextPageRequest, list);
        //    }

        //    foreach (var item in listItems)
        //    {
        //        ProcessFile(item);
        //    }
        //}

        //private void ProcessFile(ListItem item)
        //{
        //    _tracer.TrackTrace($"{item.Name}");
        //}
    }
}
