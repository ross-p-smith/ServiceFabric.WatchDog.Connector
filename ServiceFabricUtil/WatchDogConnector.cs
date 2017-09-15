namespace ServiceFabric.Helpers
{
    using System;
    using System.Fabric;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public class WatchdogConnector
    {
        private readonly ServiceContext _context;
        private readonly IServiceEventSource _eventSource;
        public WatchdogConnector(ServiceContext context, IServiceEventSource eventSource)
        {
            this._context = context;
            this._eventSource = eventSource;
        }

        /// <summary>
        /// Registers health checks with the watchdog service.
        /// </summary>
        /// <param name="uniqueHealthCheckName">Unique name of the Health check</param>
        /// <param name="suffixPath">The name of the controller to call to check health e.g. api\Health</param>
        /// <returns>A Task that represents outstanding operation.</returns>
        public async Task<bool> RegisterHealthCheckAsync(string uniqueHealthCheckName, string suffixPath)
        {
            bool result = false;
            HttpClient client = new HttpClient();
            string json =
                $"{{\"name\":\"{uniqueHealthCheckName}\",\"serviceName\": \"{this._context.ServiceName}\",\"partition\": \"{this._context.PartitionId}\",\"frequency\": \"{TimeSpan.FromMinutes(2)}\",\"suffixPath\": \"{suffixPath}\",\"method\": {{ \"Method\": \"GET\" }}, \"expectedDuration\": \"00:00:00.2000000\",\"maximumDuration\": \"00:00:05\" }}";

            // Called from RunAsync, don't let an exception out so the service will start, but log the exception because the service won't work.
            try
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, "http://localhost:19081/Watchdog/WatchdogService/healthcheck");
                request.Content = new StringContent(json, Encoding.Default, "application/json");

                HttpResponseMessage msg = await client.SendAsync(request);

                // Log a success or error message based on the returned status code.
                if (HttpStatusCode.OK == msg.StatusCode)
                {
                    this._eventSource.Trace(nameof(this.RegisterHealthCheckAsync), Enum.GetName(typeof(HttpStatusCode), msg.StatusCode));
                    result = true;
                }
                else
                {
                    this._eventSource.Error(nameof(this.RegisterHealthCheckAsync), Enum.GetName(typeof(HttpStatusCode), msg.StatusCode));
                    this._eventSource.Trace(nameof(this.RegisterHealthCheckAsync), json ?? "<null JSON>");
                }
            }
            catch (Exception ex)
            {
                this._eventSource.Error($"Exception: {ex.Message} at {ex.StackTrace}.");
            }

            return result;
        }

        /// <summary>
        /// Registers metrics with the watchdog service.
        /// </summary>
        /// <param name="appServiceUriSection">The URI part that the watchdog should call</param>
        /// <param name="stateful"></param>
        /// <returns>A Task that represents outstanding operation.</returns>
        public async Task<bool> RegisterMetricsAsync(string appServiceUriSection, bool stateful)
        {
            bool result = false;
            string uri = $"http://localhost:19081/Watchdog/WatchdogService/metrics/{appServiceUriSection}";

            try
            {
                // The URI to register with the watchdog is important. There are two options:
                // The first URI will return the load metrics for a single partition. This is useful for a stateful partition where the primary
                // registered with the watchdog each time it starts. 
                // The second URI will report for each replica within a partition. This is useful for a stateless partition where it is desired to see 
                // the load for each replica within the partition.
                if (stateful)
                {
                    uri = string.Concat(uri, $"/{this._context.PartitionId}");
                }

                // Now register them with the watchdog service.
                HttpClient httpClient = new HttpClient();
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, uri);
                request.Content = new StringContent("[\"RPS\", \"Failures\", \"Latency\", \"ItemCount\"]", Encoding.Default, "application/json");

                HttpResponseMessage msg = await httpClient.SendAsync(request);

                // Log a success or error message based on the returned status code.
                if (HttpStatusCode.OK == msg.StatusCode)
                {
                    this._eventSource.Trace(nameof(this.RegisterMetricsAsync), Enum.GetName(typeof(HttpStatusCode), msg.StatusCode));
                    result = true;
                }
                else
                {
                    this._eventSource.Error(nameof(this.RegisterMetricsAsync), Enum.GetName(typeof(HttpStatusCode), msg.StatusCode));
                }
            }
            catch (Exception ex)
            {
                this._eventSource.ServiceMessage(this._context, ex.Message);
            }

            return result;
        }
    }
}
