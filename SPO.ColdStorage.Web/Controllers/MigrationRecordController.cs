using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Entities.DBEntities;

namespace SPO.ColdStorage.Web.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MigrationRecordController : ControllerBase
    {
        private readonly ILogger<MigrationRecordController> _logger;
        private readonly SPOColdStorageDbContext _context;

        public MigrationRecordController(ILogger<MigrationRecordController> logger, SPOColdStorageDbContext context)
        {
            _logger = logger;
            this._context = context;
        }

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
                    .ToListAsync();
            }
            
        }
    }
}