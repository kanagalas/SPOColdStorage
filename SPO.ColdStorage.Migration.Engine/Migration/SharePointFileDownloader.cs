﻿using Microsoft.Identity.Client;
using SPO.ColdStorage.Entities.Configuration;
using SPO.ColdStorage.Migration.Engine.Model;
using System.Net.Http.Headers;

namespace SPO.ColdStorage.Migration.Engine.Migration
{
    /// <summary>
    /// Downloads files from SharePoint to local file-system
    /// </summary>
    public class SharePointFileDownloader : BaseComponent
    {
        private readonly IConfidentialClientApplication _app;
        private readonly HttpClient _client;
        public SharePointFileDownloader(IConfidentialClientApplication app, Config config, DebugTracer debugTracer) : base(config, debugTracer)
        {
            _app = app;
            _client = new HttpClient();
        }

        /// <summary>
        /// Download file & return temp file-name + size
        /// </summary>
        /// <returns>Temp file-path and size</returns>
        /// <remarks>
        /// Uses manual HTTP calls as CSOM doesn't work with files > 2gb. 
        /// This routine writes 2mb chunks at a time to a temp file from HTTP response.
        /// </remarks>
        public async Task<(string, long)> DownloadFileToTempDir(SharePointFileInfo sharePointFile)
        {
            // Write to temp file
            var tempFileName = GetTempFileNameAndCreateDir(sharePointFile);

            _tracer.TrackTrace($"Downloading SharePoint file '{sharePointFile.FullSharePointUrl}'...", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);

            var auth = await _app.AuthForSharePointOnline(_config.BaseServerAddress);
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
            var url = $"{sharePointFile.WebUrl}/_api/web/GetFileByServerRelativeUrl('{sharePointFile.ServerRelativeFilePath}')/OpenBinaryStream";

            long fileSize = 0;

            // Get response but don't buffer full content (which will buffer overlflow for large files)
            using (var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                using (var streamToReadFrom = await response.Content.ReadAsStreamAsync())
                using (var streamToWriteTo = File.Open(tempFileName, FileMode.Create))
                {
                    await streamToReadFrom.CopyToAsync(streamToWriteTo);
                    fileSize = streamToWriteTo.Length;
                }
            }

            _tracer.TrackTrace($"Wrote {fileSize.ToString("N0")} bytes to '{tempFileName}'.", Microsoft.ApplicationInsights.DataContracts.SeverityLevel.Verbose);

            return (tempFileName, fileSize);
        }

        public static string GetTempFileNameAndCreateDir(SharePointFileInfo sharePointFile)
        {
            var tempFileName = Path.GetTempPath() + @"\SpoColdStorageMigration\" + DateTime.Now.Ticks + @"\" + sharePointFile.ServerRelativeFilePath.Replace("/", @"\");
            var tempFileInfo = new FileInfo(tempFileName);
            Directory.CreateDirectory(tempFileInfo.DirectoryName!);

            return tempFileName;
        }
    }
}
