﻿using Azure.Storage.Blobs;
using Azure.Storage.Sas;
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
    /// <summary>
    /// Handles React app requests for app configuration
    /// </summary>
    [Microsoft.AspNetCore.Authorization.Authorize]
    [ApiController]
    [Route("[controller]")]
    public class AppConfigurationController : ControllerBase
    {
        private readonly ILogger<AppConfigurationController> _logger;
        private readonly SPOColdStorageDbContext _context;
        private readonly Config _config;

        public AppConfigurationController(ILogger<AppConfigurationController> logger, SPOColdStorageDbContext context, Config config)
        {
            _logger = logger;
            this._context = context;
            this._config = config;
        }


        // Get storage configuration to read blobs
        // GET: AppConfiguration/GetStorageInfo
        [HttpGet("[action]")]
        public ActionResult<StorageInfo> GetStorageInfo()
        {
            var client = new BlobServiceClient(_config.ConnectionStrings.Storage);

            // Generate a new shared-access-signature
            var sasUri = client.GenerateAccountSasUri(AccountSasPermissions.List | AccountSasPermissions.Read,
                DateTime.Now.AddDays(1),
                AccountSasResourceTypes.Container | AccountSasResourceTypes.Object);

            // Return for react app
            return new StorageInfo
            {
                AccountURI = client.Uri.ToString(),
                SharedAccessToken = sasUri.Query,
                ContainerName = _config.BlobContainerName
            };
        }

        // GET: AppConfiguration/GetGetMigrationTargets
        [HttpGet("[action]")]
        public async Task<ActionResult<IEnumerable<TargetMigrationSiteDTO>>> GetMigrationTargets()
        {
            var targets = await _context.TargetSharePointSites.ToListAsync();
            var returnList = new List<TargetMigrationSiteDTO>();
            foreach (var target in targets)
                returnList.Add(new TargetMigrationSiteDTO(target));

            return returnList;
        }

        /// <summary>
        /// Set migration config
        /// </summary>
        /// <param name="targets">List of sites + site config</param>
        /// <returns></returns>
        // POST: AppConfiguration/SetMigrationTargets
        [HttpGet("[action]")]
        public async Task<ActionResult> SetMigrationTargets(List<TargetMigrationSiteDTO> targets)
        {
            if (targets == null || targets.Count == 0)
            {
                return BadRequest($"{nameof(targets)} is null");
            }
            foreach (var target in targets)
            {
                if (!target.IsValid)
                {
                    return BadRequest("Invalid config data");
                }
            }
            // Verify auth works with 1st item
            try
            {
                await AuthUtils.GetClientContext(_config, targets[0].RootURL);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating authentication to SharePoint Online");
                return BadRequest($"Got '{ex.Message}' trying to get a token for SPO authentication. Check service configuration.");
            }


            // Remove old target configuration & set new
            var oldTargetSites = await _context.TargetSharePointSites.ToListAsync();
            _context.TargetSharePointSites.RemoveRange(oldTargetSites);

            // Verify each site exists
            foreach (var target in targets)
            {
                try
                {
                    var siteContext = await AuthUtils.GetClientContext(_config, target.RootURL);
                    siteContext.Load(siteContext.Web);
                    await siteContext.ExecuteQueryAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error validating site");
                    return BadRequest($"Got '{ex.Message}' validating SPO site URL '{target}'. It's not a valid SharePoint site-collection URL?");
                }

                // Assuming no error, save to SQL
                _context.TargetSharePointSites.Add(target);
            }
            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}