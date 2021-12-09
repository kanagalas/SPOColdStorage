using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Entities.Configuration;
using SPO.ColdStorage.Entities.DBEntities;

namespace SPO.ColdStorage.Web.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MigrationRecordController : ControllerBase
    {
        private readonly ILogger<MigrationRecordController> _logger;
        private readonly SPOColdStorageDbContext _context;
        private readonly Config _config;

        public MigrationRecordController(ILogger<MigrationRecordController> logger, SPOColdStorageDbContext context, Config config)
        {
            _logger = logger;
            this._context = context;
            this._config = config;
        }

        // Search for migration log by keyword
        // GET: MigrationRecord
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FileMigrationCompletedLog>>> GetSuccesfulMigrations(string keyWord)
        {
            if (string.IsNullOrEmpty(keyWord))
            {
                return BadRequest("No search term defined");
            }
            else
            {
                return await _context.FileMigrationsCompleted
                    .Where(m => m.File.Url.Contains(keyWord))
                    .Include(m => m.File)
                        .ThenInclude(f=> f.Web)
                            .ThenInclude(w=> w.Site)
                    .ToListAsync();
            }
        }

        // Get storage configuration to read blobs
        // GET: MigrationRecord/GetStorageInfo
        [HttpGet("[action]")]
        public ActionResult<StorageInfo> GetStorageInfo()
        {
            var client = new BlobServiceClient(_config.ConnectionStrings.Storage);
            var sasUri = client.GenerateAccountSasUri(AccountSasPermissions.List | AccountSasPermissions.Read, 
                DateTime.Now.AddDays(1), 
                AccountSasResourceTypes.Container);
            return new StorageInfo
            {
                AccountURI = client.Uri.ToString(),
                SharedAccessToken = sasUri.Query,
                ContainerName = _config.BlobContainerName
            };
        }
    }
}