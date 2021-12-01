using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Migration.Engine.Model;

namespace SPO.ColdStorage.Migration.Engine.Migration
{
    public class SharePointFileProcessor : BaseComponent
    {
        public SharePointFileProcessor(Config config) : base(config)
        {
        }

        public async Task ProcessFileContent(SharePointFileInfo sharePointFileInfo)
        {
            var indexClient = CreateSearchIndexClient();

            // DeleteIndexIfExists(_config.SearchIndexName, indexClient);
            CreateOrUpdateIndex(_config.SearchIndexName, indexClient);

            var searchClient = indexClient.GetSearchClient(_config.SearchIndexName);

            IndexDocumentsBatch<FileSearchModel> batch = IndexDocumentsBatch.Create(
                IndexDocumentsAction.Upload(new FileSearchModel(sharePointFileInfo))
            );

            IndexDocumentsResult result;
            try
            {
                result = await searchClient.IndexDocumentsAsync(batch);
            }
            catch (RequestFailedException ex)
            {
                _tracer.TrackException(ex);
                throw;
            }
        }
        private void DeleteIndexIfExists(string indexName, SearchIndexClient indexClient)
        {
            try
            {
                if (indexClient.GetIndex(indexName) != null)
                {
                    indexClient.DeleteIndex(indexName);
                }
            }
            catch (RequestFailedException e) when (e.Status == 404)
            {
                // Throw an exception if the index name isn't found
                Console.WriteLine("The index doesn't exist. No deletion occurred.");
            }
        }
        private SearchIndexClient CreateSearchIndexClient()
        {
            string searchServiceEndPoint = _config.SearchServiceEndPoint;
            string adminApiKey = _config.SearchServiceAdminApiKey;

            var indexClient = new SearchIndexClient(new Uri(searchServiceEndPoint), new AzureKeyCredential(adminApiKey));
            return indexClient;
        }
        private SearchClient CreateSearchClientForQueries(string indexName)
        {
            string searchServiceEndPoint = _config.SearchServiceEndPoint;
            string queryApiKey = _config.SearchServiceQueryApiKey;

            SearchClient searchClient = new SearchClient(new Uri(searchServiceEndPoint), indexName, new AzureKeyCredential(queryApiKey));
            return searchClient;
        }
        private static void CreateOrUpdateIndex(string indexName, SearchIndexClient indexClient)
        {
            FieldBuilder fieldBuilder = new FieldBuilder();
            var searchFields = fieldBuilder.Build(typeof(FileSearchModel));

            var definition = new SearchIndex(indexName, searchFields);

            indexClient.CreateOrUpdateIndex(definition);
        }
    }
}