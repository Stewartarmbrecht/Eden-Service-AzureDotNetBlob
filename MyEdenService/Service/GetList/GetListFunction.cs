namespace MyEdenSolution.MyEdenService.Service
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using MyEdenSolution.MyEdenService.Service;
    using MyEdenSolution.Common;
    using MyEdenSolution.Common.Blobs;
    using MyEdenSolution.Common.Events;
    using MyEdenSolution.Common.UserAuthentication;
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
        /// Gets a list of MyEdenService notes for a user.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <param name="log">Logger used for logging.</param>
        /// <returns>Collection of MyEdenService note summarites. An instance of the <see cref="GetListItem"/> class.</returns>
        [FunctionName("ListMyEdenService")]
        public async Task<IActionResult> GetList(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "MyEdenService")]HttpRequest req,
            ILogger log)
        {
            try
            {
                // get the user ID
                if (!await this.UserAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult).ConfigureAwait(false))
                {
                    return responseResult;
                }

                // list the MyEdenService notes
                var blobs = await this.BlobRepository.ListBlobsInFolderAsync(MyEdenServiceBlobContainerName, userId).ConfigureAwait(false);
                var blobSummaries = blobs
                    .Select(b => new GetListItem
                    {
                        Id = b.BlobName.Split('/')[1],
                        Preview = b.Properties.ContainsKey(TranscriptMetadataName) ? b.Properties[TranscriptMetadataName].Truncate(TranscriptPreviewLength) : string.Empty,
                    })
                    .ToList();

                var MyEdenServiceNoteSummaries = new GetListResponse();
                MyEdenServiceNoteSummaries.AddRange(blobSummaries);

                return new ObjectResult(MyEdenServiceNoteSummaries);
            }
            catch (Exception ex)
            {
                log.LogError(ex, UnhandledExceptionError);
                throw;
            }
        }
    }
}
