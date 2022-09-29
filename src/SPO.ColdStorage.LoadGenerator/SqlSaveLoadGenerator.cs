using SPO.ColdStorage.Migration.Engine;
using SPO.ColdStorage.Migration.Engine.SnapshotBuilder;
using SPO.ColdStorage.Models;

namespace SPO.ColdStorage.LoadGenerator
{
    internal class SqlSaveLoadGenerator
    {
        public static async Task Go(Entities.Configuration.Config config)
        {
            var tasks = new List<Task>();

            for (int i = 0; i < 100; i++)
            {
                tasks.Add(Insert(config, 10000));
            }

            await Task.WhenAll(tasks);
        }

        public static async Task Insert(Entities.Configuration.Config config, int docsToInsert)
        {

            var list = new List<SharePointFileInfoWithList>();
            var spList = new SiteList(){ ServerRelativeUrl = $"/list{DateTime.Now.Ticks}" };

            for (int i = 0; i < docsToInsert; i++)
            {
                list.Add(new DocumentSiteWithMetadata 
                { 
                    AccessCount = i,
                    Author = $"Author {i}",
                    List = spList,
                    DriveId = DateTime.Now.Ticks.ToString(),
                    FileSize = i,
                    GraphItemId = DateTime.Now.Ticks.ToString(),
                    VersionCount = i
                });
            }


            Console.WriteLine("Saving fakes");
            StagingFilesMigrator stagingFilesMigrator = new();

            await list.InsertFilesAsync(config, stagingFilesMigrator, DebugTracer.ConsoleOnlyTracer());

        }
    }
}
