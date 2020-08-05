namespace MyEdenSolution.MyEdenService.Service
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Azure.WebJobs;
    using Microsoft.Azure.WebJobs.Extensions.Http;
    using Microsoft.Extensions.Logging;

    /// <summary>
    /// Contains the operation for beginning the add of an MyEdenService file.
    /// </summary>
    public partial class Functions
    {
        /// <summary>
        /// Gets metadata and URL to download MyEdenService file.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <param name="log">Logger used for logging.</param>
        /// <param name="id">The id of the MyEdenService file to get the data for.</param>
        /// <returns>Metadata about the MyEdenService file and the URL to download.</returns>
        [FunctionName("GetMyEdenService")]
        public async Task<IActionResult> Get(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "MyEdenService/{id}")]HttpRequest req,
            ILogger log,
            string id)
        {
            // get the MyEdenService note
            try
            {
                // get the user ID
                if (!await this.UserAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult).ConfigureAwait(false))
                {
                    return responseResult;
                }

                var MyEdenServiceBlob = await this.BlobRepository.GetBlobAsync(MyEdenServiceBlobContainerName, $"{userId}/{id}").ConfigureAwait(false);
                if (MyEdenServiceBlob == null)
                {
                    return new NotFoundResult();
                }

                Uri blobDownloadUrl = this.BlobRepository.GetBlobDownloadUrl(MyEdenServiceBlob);

                var response = new GetResponse
                {
                    Id = id,
                    MyEdenServiceUrl = blobDownloadUrl,
                    Transcript = MyEdenServiceBlob.Properties.ContainsKey(TranscriptMetadataName) ? MyEdenServiceBlob.Properties[TranscriptMetadataName] : null,
                };
                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                log.LogError(ex, UnhandledExceptionError);
                throw;
            }
        }
    }
}
