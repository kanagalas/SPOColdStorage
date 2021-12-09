using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Entities.Configuration;
using SPO.ColdStorage.Entities.DBEntities;
using SPO.ColdStorage.Migration.Engine;
using SPO.ColdStorage.Web.Models;

namespace SPO.ColdStorage.Web.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ApiController]
    [Route("[controller]")]
    public class MigrationController : ControllerBase
    {
        private readonly ILogger<MigrationController> _logger;
        private readonly SPOColdStorageDbContext _context;
        private readonly Config _config;

        public MigrationController(ILogger<MigrationController> logger, SPOColdStorageDbContext context, Config config)
        {
            _logger = logger;
            this._context = context;
            this._config = config;
        }

        // GET: Migration
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TargetMigrationSite>>> GetMigrations()
        {
            return await _context.TargetSharePointSites.ToListAsync();
        }

        // POST: Migration
        [HttpPost]
        public async Task<ActionResult> SetMigrations(MigrationsConfig config)
        {
            if (config == null)
            {
                return BadRequest($"{nameof(config)} is null");
            }

            // Remove old & set new
            var oldTargetSites = await _context.TargetSharePointSites.ToListAsync();
            _context.TargetSharePointSites.RemoveRange(oldTargetSites);
            
            // Verify each site exists
            foreach (var siteUrl in config.TargetSites)
            {
                // Verify each site
                var siteContext = await AuthUtils.GetClientContext(_config, siteUrl);
                siteContext.Load(siteContext.Web);
                await siteContext.ExecuteQueryAsync();

                // Assuming no error, save to SQL
                _context.TargetSharePointSites.Add(new TargetMigrationSite { RootURL = siteUrl });
            }
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}