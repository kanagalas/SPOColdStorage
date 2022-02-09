using Microsoft.SharePoint.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SPO.ColdStorage.Migration.Engine.Utils
{
    public static class CSOMExtensions
    {
        public static async Task ExecuteQueryAsyncWithThrottleRetries(this ClientContext clientContext)
        {
            int retryCount = 10;
            int retryAttempts = 0;
            int backoffIntervalSeconds = 1;
            int retryAfterInterval = 0;
            bool retry = false;
            ClientRequestWrapper? wrapper = null;


            // Do while retry attempt is less than retry count
            while (retryAttempts < retryCount)
            {
                try
                {
                    if (!retry)
                    {
                        await clientContext.ExecuteQueryAsync();
                        return;
                    }
                    else
                    {
                        //increment the retry count
                        retryAttempts++;

                        // retry the previous request using wrapper
                        if (wrapper != null && wrapper.Value != null)
                        {
                            clientContext.RetryQuery(wrapper.Value);
                            return;
                        }
                        // retry the previous request as normal
                        else
                        {
                            clientContext.ExecuteQuery();
                            return;
                        }
                    }
                }
                catch (WebException ex)
                {
                    var response = ex.Response as HttpWebResponse;
                    // Check if request was throttled - http status code 429
                    // Check is request failed due to server unavailable - http status code 503
                    if (response != null && (response.StatusCode == (HttpStatusCode)429 || response.StatusCode == (HttpStatusCode)503))
                    {
                        wrapper = (ClientRequestWrapper)ex.Data["ClientRequest"];
                        retry = true;

                        // Determine the retry after value - use the `Retry-After` header when available
                        string retryAfterHeader = response.GetResponseHeader("Retry-After");
                        if (!string.IsNullOrEmpty(retryAfterHeader))
                        {
                            if (!Int32.TryParse(retryAfterHeader, out retryAfterInterval))
                            {
                                retryAfterInterval = backoffIntervalSeconds;
                            }
                        }
                        else
                        {
                            retryAfterInterval = backoffIntervalSeconds;
                        }

                        Console.WriteLine($"Got throttled. Sleeping for {retryAfterInterval} seconds.");

                        // Delay for the requested seconds
                        Thread.Sleep(retryAfterInterval * 1000);

                        // Increase counters
                        backoffIntervalSeconds = backoffIntervalSeconds * 2;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            throw new Exception($"Maximum retry attempts {retryCount}, has be attempted.");

        }
    }
}
