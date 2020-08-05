namespace MyEdenSolution.MyEdenService.Service
{
    using System;
    using System.Threading.Tasks;
    using MyEdenSolution.Common.Events.MyEdenService;
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
        /// Deletes an MyEdenService note from the repository.
        /// </summary>
        /// <param name="req">The request.</param>
        /// <param name="log">Logger used for logging.</param>
        /// <param name="id">The id of the MyEdenService file to delete.</param>
        /// <returns>No content result if successful.</returns>
        [FunctionName("DeleteMyEdenService")]
        public async Task<IActionResult> Delete(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "MyEdenService/{id}")]HttpRequest req,
            ILogger log,
            string id)
        {
            // delete the MyEdenService note
            try
            {
                // get the user ID
                if (!await this.UserAuthenticationService.GetUserIdAsync(req, out var userId, out var responseResult).ConfigureAwait(false))
                {
                    return responseResult;
                }

                // delete the blog
                await this.BlobRepository.DeleteBlobAsync(MyEdenServiceBlobContainerName, $"{userId}/{id}").ConfigureAwait(false);

                // fire an event into the Event Grid topic
                var subject = $"{userId}/{id}";
                await this.EventGridPublisherService.PostEventGridEventAsync(MyEdenServiceEvents.MyEdenServiceDeleted, subject, new MyEdenServiceDeletedEventData()).ConfigureAwait(false);
                return new NoContentResult();
            }
            catch (Exception ex)
            {
                log.LogError(ex, UnhandledExceptionError);
                throw;
            }
        }
    }
}
