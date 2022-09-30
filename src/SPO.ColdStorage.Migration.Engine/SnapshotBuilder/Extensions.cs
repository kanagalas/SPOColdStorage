using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Entities.Configuration;
using SPO.ColdStorage.Entities.DBEntities;
using SPO.ColdStorage.Models;

namespace SPO.ColdStorage.Migration.Engine.SnapshotBuilder
{
    public static class Extensions
    {
        private static SemaphoreSlim ss = new(0, 1); 
        public static async Task InsertFilesAsync(this List<SharePointFileInfoWithList> files, Config config, StagingFilesMigrator stagingFilesMigrator, DebugTracer tracer)
        {
            await ss.WaitAsync();

            try
            {
                using (var db = new SPOColdStorageDbContext(config))
                {
                    var executionStrategy = db.Database.CreateExecutionStrategy();

                    try
                    {
                        await executionStrategy.Execute(async () =>
                        {
                            using (var trans = await db.Database.BeginTransactionAsync())
                            {
                                var blockGuid = Guid.NewGuid();
                                var inserted = DateTime.Now;

                            // Insert staging data
                                var stagingFiles = new List<StagingTempFile>();
                                int i = 0;
                                foreach (var insertedFile in files)
                                {
                                    var f = new StagingTempFile(insertedFile, blockGuid, inserted);
                                    stagingFiles.Add(f);

#if DEBUG
                                    if (i > 0 && i % 1000 == 0)
                                    {
                                        Console.WriteLine($"{i.ToString("N0")}/{files.Count.ToString("N0")}");
                                    }
#endif
                                    i++;
                                }
                                await db.StagingFiles.AddRangeAsync(stagingFiles);
                                await db.SaveChangesAsync();

                            // Merge from staging to tables
                                var inserts = stagingFilesMigrator.MigrateBlockAndCleanFromStaging(db, blockGuid);

                                await trans.CommitAsync();
                            }
                        });

                    }
                    catch (SqlException ex)
                    {
                        tracer.TrackException(ex);
                        tracer.TrackTrace($"Got fatal SQL error saving file info to SQL: {ex.Message}", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Critical);
                    }
                }
            }
            finally
            {
                ss.Release();
            }
        }
    }
}
