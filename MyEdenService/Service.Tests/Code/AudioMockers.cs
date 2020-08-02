namespace MyEdenSolution.MyEdenService.Service.Tests.Unit
{
    using MyEdenSolution.Common.Events;
    using MyEdenSolution.Common.Events.MyEdenService;
    using MyEdenSolution.Common.Fakes;
    using MyEdenSolution.Common.UserAuthentication;
    using Microsoft.AspNetCore.Http;
    using Moq;
    using Api = MyEdenSolution.MyEdenService.Service;

    /// <summary>
    /// Helper functions to get mocks for MyEdenService unit testing.
    /// </summary>
    public static class MyEdenServiceMockers
    {
        /// <summary>
        /// Gets a mocked MyEdenService add complete request.
        /// </summary>
        /// <returns>Mock http request with an MyEdenService add complete request in the body.</returns>
        public static Mock<HttpRequest> GetMockAddCompleteRequest()
        {
            var requestBody = new Api.AddCompleteRequest()
            {
                CategoryId = Mockers.DefaultCategoryName,
            };

            return Mockers.MockRequest(requestBody);
        }

        /// <summary>
        /// Gets a mocked event grid update transcription request.
        /// </summary>
        /// <returns>Mock http request that would come from the event grid for an MyEdenService created event.</returns>
        public static Mock<HttpRequest> GetMockEventGridMyEdenServiceCreatedRequest()
        {
            var requestBody = new EventGridRequest<MyEdenServiceCreatedEventData>()
            {
                UserId = Mockers.DefaultUserId,
                ItemId = Mockers.DefaultId,
                Event = new EventGridEvent<MyEdenServiceCreatedEventData>()
                {
                    Data = new MyEdenServiceCreatedEventData()
                    {
                        Category = Mockers.DefaultId,
                    },
                    EventTime = System.DateTime.Now,
                    Id = System.Guid.NewGuid().ToString(),
                    EventType = "MyEdenServiceCreated",
                    Subject = $"{Mockers.DefaultUserId}/{Mockers.DefaultId}",
                    Topic = "faketopic",
                },
            };

            var headers = new HeaderDictionary();

            return Mockers.MockRequest(requestBody, headers);
        }

        /// <summary>
        /// Gets an MyEdenService operations class with a blob uploaded to the mock
        /// blob repository.
        /// </summary>
        /// <returns>An instance of the <see cref="Api.Functions"/> class.</returns>
        public static Api.Functions GetApiFunctionsWithBlobUploaded()
        {
            return GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeBlobRepository,
                out Mock<IEventGridPublisherService> mockEventPub);
        }

        /// <summary>
        /// Gets an MyEdenService operations class with a blob uploaded to the mock
        /// blob repository.
        /// </summary>
        /// <param name="fakeBlobRepo">Returns the fake blob repository with the added blob.</param>
        /// <returns>An instance of the <see cref="Api.Functions"/> class.</returns>
        public static Api.Functions GetApiFunctionsWithBlobUploaded(
            out FakeBlobRepository fakeBlobRepo)
        {
            return GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventPub);
        }

        /// <summary>
        /// Gets an MyEdenService operations class with a blob uploaded to the mock
        /// blob repository.
        /// </summary>
        /// <param name="fakeBlobRepo">Returns the fake blob repository with the added blob.</param>
        /// <param name="mockEventPub">Returns the mock event publisher service.</param>
        /// <returns>An instance of the <see cref="Api.Functions"/> class.</returns>
        public static Api.Functions GetApiFunctionsWithBlobUploaded(
            out FakeBlobRepository fakeBlobRepo,
            out Mock<IEventGridPublisherService> mockEventPub)
        {
            return GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out fakeBlobRepo,
                out mockEventPub);
        }

        /// <summary>
        /// Gets an MyEdenService operations class with a blob uploaded to the mock
        /// blob repository.
        /// </summary>
        /// <param name="mockUserAuth">Returns the mock user auth service.</param>
        /// <returns>An instance of the <see cref="Api.Functions"/> class.</returns>
        public static Api.Functions GetApiFunctionsWithBlobUploaded(
            out Mock<IUserAuthenticationService> mockUserAuth)
        {
            return GetApiFunctionsWithBlobUploaded(
                out mockUserAuth,
                out FakeBlobRepository fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventPub,
                out Mock<IEventGridSubscriberService> mockEventSub,
                out Mock<Api.IMyEdenServiceTranscriptionService> mockTranscriptService);
        }

        /// <summary>
        /// Gets an MyEdenService operations class with a blob uploaded to the mock
        /// blob repository.
        /// </summary>
        /// <param name="mockUserAuth">Returns the mock user auth.</param>
        /// <param name="fakeBlobRepo">Returns the fake blob repository with the added blob.</param>
        /// <param name="mockEventPub">Returns the fake event publisher.</param>
        /// <returns>An instance of the <see cref="Api.Functions"/> class.</returns>
        public static Api.Functions GetApiFunctionsWithBlobUploaded(
            out Mock<IUserAuthenticationService> mockUserAuth,
            out FakeBlobRepository fakeBlobRepo,
            out Mock<IEventGridPublisherService> mockEventPub)
        {
            mockUserAuth = Mockers.MockUserAuth();

            mockEventPub = new Mock<IEventGridPublisherService>();
            var mockEventSub = new Mock<IEventGridSubscriberService>();
            var mockMyEdenServiceTranscriptionService = new Mock<Api.IMyEdenServiceTranscriptionService>();

            fakeBlobRepo = new FakeBlobRepository();
            fakeBlobRepo.AddFakeBlob(Mockers.MyEdenServiceContainerName, $"{Mockers.DefaultUserId}/{Mockers.DefaultId}");

            return new Api.Functions(
                mockUserAuth.Object,
                fakeBlobRepo,
                mockEventSub.Object,
                mockEventPub.Object,
                mockMyEdenServiceTranscriptionService.Object);
        }

        /// <summary>
        /// Gets an MyEdenService worker functions class with a blob uploaded to the mock
        /// blob repository.
        /// </summary>
        /// <param name="mockUserAuth">Returns the mock user auth.</param>
        /// <param name="fakeBlobRepo">Returns the fake blob repository with the added blob.</param>
        /// <param name="mockEventPub">Returns the fake event publisher.</param>
        /// <param name="mockEventSub">Returns the fake event subscriber.</param>
        /// <param name="mockMyEdenServiceTranscriptionService">Returns the mock MyEdenService transcription service.</param>
        /// <returns>An instance of the <see cref="Api.Functions"/> class.</returns>
        public static Api.Functions GetApiFunctionsWithBlobUploaded(
            out Mock<IUserAuthenticationService> mockUserAuth,
            out FakeBlobRepository fakeBlobRepo,
            out Mock<IEventGridPublisherService> mockEventPub,
            out Mock<IEventGridSubscriberService> mockEventSub,
            out Mock<Api.IMyEdenServiceTranscriptionService> mockMyEdenServiceTranscriptionService)
        {
            mockUserAuth = Mockers.MockUserAuth();

            mockEventPub = new Mock<IEventGridPublisherService>();
            mockEventSub = new Mock<IEventGridSubscriberService>();
            mockMyEdenServiceTranscriptionService = new Mock<Api.IMyEdenServiceTranscriptionService>();

            fakeBlobRepo = new FakeBlobRepository();
            fakeBlobRepo.AddFakeBlob(Mockers.MyEdenServiceContainerName, $"{Mockers.DefaultUserId}/{Mockers.DefaultId}");

            return new Api.Functions(
                mockUserAuth.Object,
                fakeBlobRepo,
                mockEventSub.Object,
                mockEventPub.Object,
                mockMyEdenServiceTranscriptionService.Object);
        }
    }
}