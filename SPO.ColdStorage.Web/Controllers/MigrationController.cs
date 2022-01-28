using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SPO.ColdStorage.Entities;
using SPO.ColdStorage.Entities.Configuration;
using SPO.ColdStorage.Entities.DBEntities;
using SPO.ColdStorage.Migration.Engine;
using SPO.ColdStorage.Migration.Engine.Model;
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
        public async Task<ActionResult<IEnumerable<TargetMigrationSiteDTO>>> GetMigrations()
        {
            var targets = await _context.TargetSharePointSites.ToListAsync();
            var returnList = new List<TargetMigrationSiteDTO>();
            foreach (var target in targets)
                returnList.Add(new TargetMigrationSiteDTO(target));

            return returnList;
        }

        // POST: Migration
        [HttpPost]
        public async Task<ActionResult> SetMigrations(MigrationsConfig config)
        {
            if (config == null || config.TargetSites.Count == 0)
            {
                return BadRequest($"{nameof(config)} is null");
            }

            // Remove old & set new
            var oldTargetSites = await _context.TargetSharePointSites.ToListAsync();
            _context.TargetSharePointSites.RemoveRange(oldTargetSites);

            // Verify auth works
            try
            {
                await AuthUtils.GetClientContext(_config, config.TargetSites[0]);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating auth");
                return BadRequest($"Got '{ex.Message}' trying to get a token for SPO auth. Check config.");
            }

            // Verify each site exists
            foreach (var siteUrl in config.TargetSites)
            {
                try
                {
                    var siteContext = await AuthUtils.GetClientContext(_config, siteUrl);
                    siteContext.Load(siteContext.Web);
                    await siteContext.ExecuteQueryAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating site");
                    return BadRequest($"Got '{ex.Message}' validating SPO site URL '{siteUrl}'. It's not a valid SharePoint site-collection URL?");
                }

                // Assuming no error, save to SQL
                _context.TargetSharePointSites.Add(new TargetMigrationSite { RootURL = siteUrl });
            }
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}