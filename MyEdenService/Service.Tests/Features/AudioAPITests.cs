namespace MyEdenSolution.MyEdenService.Service.Tests.Features
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;
    using System.Threading.Tasks;
    using MyEdenSolution.MyEdenService.Service;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Newtonsoft.Json;

    /// <summary>
    /// Contains end to end tests for the MyEdenService API.
    /// </summary>
    [TestClass]
    [TestCategory("Features")]
    public class MyEdenServiceApiTests
    {
        private static readonly HttpClient HttpClientInstance = new HttpClient();
        private readonly string baseUrl = Environment.GetEnvironmentVariable("FeaturesUrl");
        private readonly string defaultUserId = "developer@edentest.com";

        /// <summary>
        /// Given you have an MyEdenService note
        /// When you add the MyEdenService note through the api
        /// Then you should be able to retrieve a url to download the note
        /// And the note should have the category that you specified when completing the upload
        /// And the note should have a transcription.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task AddMyEdenServiceWithSuccess()
        {
            // Get the url to upload a new MyEdenService file.
            (string blobId, string blobUploadUrl) = await this.BeginAddMyEdenService().ConfigureAwait(false);

            // Upload the MyEdenService file to the storage service.
            await UploadFile(blobUploadUrl).ConfigureAwait(false);

            // Complete the add of the new MyEdenService file with the MyEdenService service
            await this.EndAddMyEdenService(blobId).ConfigureAwait(false);

            // Get the new MyEdenService file and validate its properties
            GetResponse getResponse = await this.GetMyEdenServiceDetail(blobId).ConfigureAwait(false);

            // Check the blob to verify it is transcribed with in 10 seconds.
            // await this.GetMyEdenServiceTranscript(getResponse).ConfigureAwait(false);
        }

        /// <summary>
        /// Given you have an MyEdenService note that you have added
        /// When you delete the MyEdenService note
        /// Then you should receive a complete message
        /// And the get operation should no longer return the note.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task DeleteMyEdenServiceWithSuccess()
        {
            // Add an MyEdenService note.
            GetResponse getResponse = await this.AddMyEdenServiceNote().ConfigureAwait(false);

            // Delete the MyEdenService note.
            await this.DeleteMyEdenService(getResponse).ConfigureAwait(false);

            // Get the deleted MyEdenService file and validate not found result.
            var missing = await this.GetMissingMyEdenServiceDetail(getResponse.Id).ConfigureAwait(false);

            Assert.IsTrue(missing);
        }

        /// <summary>
        /// Given you added multiple MyEdenService notes for multiple users
        /// When you call the list MyEdenService note operation for a single user
        /// Then you should get a list of all MyEdenService notes for that user
        /// And you should not get any of the MyEdenService notes for another user.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task ListMyEdenServiceWithSuccess()
        {
            string firstUserId = Guid.NewGuid().ToString();
            string secondUserId = Guid.NewGuid().ToString();

            // Add an MyEdenService notes for default user and second user.
            await this.AddMyEdenServiceNote(firstUserId).ConfigureAwait(false);
            await this.AddMyEdenServiceNote(firstUserId).ConfigureAwait(false);
            await this.AddMyEdenServiceNote(secondUserId).ConfigureAwait(false);
            await this.AddMyEdenServiceNote(secondUserId).ConfigureAwait(false);

            // Delete the MyEdenService note.
            var MyEdenServiceNotes = await this.ListMyEdenServiceDetail(secondUserId).ConfigureAwait(false);

            // Validate result set.
            Assert.AreEqual(2, MyEdenServiceNotes.Count);
            Assert.IsNotNull(MyEdenServiceNotes[0].Id);
            Assert.IsNotNull(MyEdenServiceNotes[1].Id);
        }

        private static async Task UploadFile(string blobUploadUrl)
        {
            var MyEdenServiceFile = await System.IO.File.ReadAllBytesAsync("no-thats-not-gonna-do-it.wav").ConfigureAwait(false);
            Assert.IsNotNull(MyEdenServiceFile);
            var uploadFile = new ByteArrayContent(MyEdenServiceFile);
            using var uploadRequest = new HttpRequestMessage()
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(blobUploadUrl),
            };
            uploadRequest.Content = uploadFile;
            uploadRequest.Headers.Add("x-ms-blob-type", "BlockBlob");
            var uploadResponse = await HttpClientInstance.SendAsync(uploadRequest).ConfigureAwait(false);
            Assert.IsTrue(uploadResponse.IsSuccessStatusCode);
            return;
        }

        private async Task<GetResponse> AddMyEdenServiceNote(string userId = null)
        {
            (string blobId, string blobUploadUrl) = await this.BeginAddMyEdenService(userId).ConfigureAwait(false);

            // Upload the MyEdenService file to the storage service.
            await UploadFile(blobUploadUrl).ConfigureAwait(false);

            // Complete the add of the new MyEdenService file with the MyEdenService service
            await this.EndAddMyEdenService(blobId, userId).ConfigureAwait(false);

            // Get the new MyEdenService file and validate its properties
            GetResponse getResponse = await this.GetMyEdenServiceDetail(blobId, userId).ConfigureAwait(false);
            return getResponse;
        }

        [SuppressMessage("StyleCop.CSharp.SpacingRules", "SA1009:ClosingParenthesisMustBeSpacedCorrectly", Justification = "Reviewed.")]
        private async Task<(string blobId, string blobUploadUrl)> BeginAddMyEdenService(string userId = null)
        {
            userId ??= this.defaultUserId;
            Uri postUri = new Uri($"{this.baseUrl}?userId={userId}");
            var beginAddResponse = await HttpClientInstance.PostAsync(postUri, null).ConfigureAwait(false);
            var beginAddResponseContent = await beginAddResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            dynamic beginAddResult = JsonConvert.DeserializeObject(beginAddResponseContent);
            var blobId = (string)beginAddResult.id;
            var blobUploadUrl = (string)beginAddResult.url;
            Assert.IsNotNull(blobId);
            Assert.IsNotNull(blobUploadUrl);
            return (blobId, blobUploadUrl);
        }

        private async Task<GetResponse> GetMyEdenServiceDetail(string blobId, string userId = null)
        {
            userId ??= this.defaultUserId;
            Uri getUrl = new Uri($"{this.baseUrl}/{blobId}?userId={userId}");
            var getResponse = await HttpClientInstance.GetAsync(getUrl).ConfigureAwait(false);
            var getResponseContent = await getResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            GetResponse getResponseBody =
                JsonConvert.DeserializeObject<GetResponse>(getResponseContent);
            Assert.AreEqual(blobId, getResponseBody.Id);
            string downloadUrlEnd = $".blob.core.windows.net/MyEdenService/{userId}/{blobId}";
            Assert.IsTrue(
                getResponseBody.MyEdenServiceUrl.ToString().Contains(downloadUrlEnd, StringComparison.Ordinal),
                $"{getResponseBody.MyEdenServiceUrl} did not contain the string {downloadUrlEnd}");
            return getResponseBody;
        }

        private async Task<GetListResponse> ListMyEdenServiceDetail(string userId = null)
        {
            userId ??= this.defaultUserId;
            Uri getUrl = new Uri($"{this.baseUrl}?userId={userId}");
            var getResponse = await HttpClientInstance.GetAsync(getUrl).ConfigureAwait(false);
            var getResponseContent = await getResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            GetListResponse MyEdenServiceNotes = JsonConvert.DeserializeObject<GetListResponse>(getResponseContent);
            return MyEdenServiceNotes;
        }

        private async Task<bool> GetMissingMyEdenServiceDetail(string blobId, string userId = null)
        {
            userId ??= this.defaultUserId;
            Uri getUrl = new Uri($"{this.baseUrl}/{blobId}?userId={userId}");
            var getResponse = await HttpClientInstance.GetAsync(getUrl).ConfigureAwait(false);
            return getResponse.StatusCode == System.Net.HttpStatusCode.NotFound;
        }

        private async Task EndAddMyEdenService(string blobId, string userId = null)
        {
            userId ??= this.defaultUserId;
            Uri endAddUrl = new Uri($"{this.baseUrl}/{blobId}?userId={userId}");
            using var endAddContent = new StringContent("{\"categoryId\":\"My Test\"}");
            var endAddResponse = await HttpClientInstance.PostAsync(endAddUrl, endAddContent).ConfigureAwait(false);
            Assert.IsTrue(endAddResponse.StatusCode == System.Net.HttpStatusCode.NoContent);
            return;
        }

        private async Task<GetResponse> GetMyEdenServiceTranscript(GetResponse MyEdenServiceNoteDetail, string userId = null)
        {
            userId ??= this.defaultUserId;
            Uri getUrl = new Uri($"{this.baseUrl}/{MyEdenServiceNoteDetail.Id}?userId={userId}");
            const string transcript = "No, that's not going to do it.";
            var transcribed = false;
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            watch.Start();
            while (watch.ElapsedMilliseconds < 10000 && !transcribed)
            {
                var getMyEdenServiceTranscriptCheckResponse = await HttpClientInstance.GetAsync(getUrl).ConfigureAwait(false);
                var getMyEdenServiceTranscriptCheckResponseContent = await getMyEdenServiceTranscriptCheckResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
                MyEdenServiceNoteDetail = JsonConvert.DeserializeObject<GetResponse>(getMyEdenServiceTranscriptCheckResponseContent);
                if (MyEdenServiceNoteDetail.Transcript == transcript)
                {
                    transcribed = true;
                }

                await Task.Delay(1000).ConfigureAwait(false);
            }

            Assert.IsTrue(transcribed, "It took longer than 10 seconds to transcribe MyEdenService file.");
            return MyEdenServiceNoteDetail;
        }

        private async Task DeleteMyEdenService(GetResponse MyEdenServiceNoteDetail, string userId = null)
        {
            userId ??= this.defaultUserId;
            Uri noteUrl = new Uri($"{this.baseUrl}/{MyEdenServiceNoteDetail.Id}?userId={userId}");
            var deleteResponse = await HttpClientInstance.DeleteAsync(noteUrl).ConfigureAwait(false);
            Assert.IsTrue(deleteResponse.StatusCode == System.Net.HttpStatusCode.NoContent);
            return;
        }
    }
}
