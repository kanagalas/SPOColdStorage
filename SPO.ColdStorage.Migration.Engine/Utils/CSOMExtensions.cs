using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine.Utils
{
    public static class CSOMExtensions
    {
        public static async Task ExecuteQueryAsyncWithThrottleRetries(this ClientContext ctx)
        {
            var retries = 0;

            try
            {
                await ctx.ExecuteQueryAsync();
            }
            catch (System.Net.WebException ex)
            {
                if (((System.Net.HttpWebResponse)ex.Response!).StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                {
                    retries++;

                    if (retries > 10)
                    {
                        throw;
                    }
                    await Task.Delay(1000 * retries);
                    Console.WriteLine($"Got throttled. Delaying {retries} seconds.");
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
