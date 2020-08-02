namespace MyEdenSolution.MyEdenService.Service.Tests.Unit
{
    using System;
    using System.Threading.Tasks;
    using MyEdenSolution.MyEdenService.Service;
    using MyEdenSolution.Common.Events;
    using MyEdenSolution.Common.Events.MyEdenService;
    using MyEdenSolution.Common.Fakes;
    using MyEdenSolution.Common.UserAuthentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Contains unit tests for the MyEdenService Service update operations.
    /// </summary>
    [TestClass]
    public class UpdateTranscriptFunctionTests
    {
        /// <summary>
        /// Given you have an MyEdenService api with an MyEdenService blob
        /// When you call the update MyEdenService transcript function
        /// Then it should update the transcript property of the blob.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithSuccessUpdatesBlobTranscriptInMetadata()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockEventGridMyEdenServiceCreatedRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService,
                out Mock<IEventGridSubscriberService> mockEventGridSubscriberService,
                out Mock<IMyEdenServiceTranscriptionService> mockMyEdenServiceTranscriptionService);

            MockEventGridSubscriberServiceDeconstructResponse(mockEventGridSubscriberService);

            mockMyEdenServiceTranscriptionService
                .Setup(s => s.GetMyEdenServiceTranscriptFromCognitiveServicesAsync(It.IsAny<System.IO.Stream>()))
                .ReturnsAsync("my transcript");

            // act
            var response = await sut.UpdateTranscript(
                mockRequest.Object,
                mockLogger.Object).ConfigureAwait(false);
            var objectResult = (OkResult)response;

            // assert
            Assert.IsNotNull(objectResult);
            Assert.AreEqual("my transcript", fakeBlobRepo.Blobs[0].Properties[Mockers.TranscriptMetadataName]);
        }

        /// <summary>
        /// Given you have an MyEdenService service with an MyEdenService blob
        /// When you call the UpdateTranscriptFunctionAsync method
        /// Then it should raise the MyEdenServiceEvents.MyEdenServiceTranscriptUpdated event.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithSuccessPublishesMyEdenServiceTranscriptUpdatedEventToEventGrid()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockEventGridMyEdenServiceCreatedRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService,
                out Mock<IEventGridSubscriberService> mockEventGridSubscriberService,
                out Mock<IMyEdenServiceTranscriptionService> mockMyEdenServiceTranscriptionService);

            MockEventGridSubscriberServiceDeconstructResponse(mockEventGridSubscriberService);

            mockMyEdenServiceTranscriptionService
                .Setup(s => s.GetMyEdenServiceTranscriptFromCognitiveServicesAsync(It.IsAny<System.IO.Stream>()))
                .ReturnsAsync("my transcript");

            // act
            var response = await sut.UpdateTranscript(
                mockRequest.Object,
                mockLogger.Object).ConfigureAwait(false);
            var objectResult = (OkResult)response;

            // assert
            mockEventGridPublisherService.Verify(
                m => m.PostEventGridEventAsync(
                    MyEdenServiceEvents.MyEdenServiceTranscriptUpdated,
                    $"{Mockers.DefaultUserId}/{Mockers.DefaultId}",
                    It.Is<MyEdenServiceTranscriptUpdatedEventData>(d => d.TranscriptPreview == "my transcript")),
                Times.Once);
        }

        /// <summary>
        /// Given you have an MyEdenService api
        /// When you call the update MyEdenService transcript function with an invalid MyEdenService blob id
        /// Then it should return a not found results.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithInvalidMyEdenServiceIdReturnsMyEdenServiceNotFound()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockEventGridMyEdenServiceCreatedRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService,
                out Mock<IEventGridSubscriberService> mockEventGridSubscriberService,
                out Mock<IMyEdenServiceTranscriptionService> mockMyEdenServiceTranscriptionService);

            fakeBlobRepo.Blobs.Clear();
            fakeBlobRepo.AddFakeBlob(Mockers.MyEdenServiceContainerName, $"{Mockers.DefaultUserId}/invalidblobid");

            MockEventGridSubscriberServiceDeconstructResponse(mockEventGridSubscriberService);

            mockMyEdenServiceTranscriptionService
                .Setup(s => s.GetMyEdenServiceTranscriptFromCognitiveServicesAsync(It.IsAny<System.IO.Stream>()))
                .ReturnsAsync("my transcript");

            // act
            var response = await sut.UpdateTranscript(
                mockRequest.Object,
                mockLogger.Object).ConfigureAwait(false);
            var objectResult = (NotFoundResult)response;

            // assert
            Assert.IsNotNull(objectResult);
        }

        /// <summary>
        /// Given you have an MyEdenService api with a blob
        /// When you call the update transcript function with an invalid user id but the correct blob id
        /// Then it should return a not found result.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithIncorrectUserIdReturnsNotFound()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockEventGridMyEdenServiceCreatedRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService,
                out Mock<IEventGridSubscriberService> mockEventGridSubscriberService,
                out Mock<IMyEdenServiceTranscriptionService> mockMyEdenServiceTranscriptionService);

            fakeBlobRepo.Blobs.Clear();
            fakeBlobRepo.AddFakeBlob(Mockers.MyEdenServiceContainerName, $"invaliduserid/{Mockers.DefaultId}");

            MockEventGridSubscriberServiceDeconstructResponse(mockEventGridSubscriberService);

            mockMyEdenServiceTranscriptionService
                .Setup(s => s.GetMyEdenServiceTranscriptFromCognitiveServicesAsync(It.IsAny<System.IO.Stream>()))
                .ReturnsAsync("my transcript");

            // act
            var response = await sut.UpdateTranscript(
                mockRequest.Object,
                mockLogger.Object).ConfigureAwait(false);
            var objectResult = (NotFoundResult)response;

            // assert
            Assert.IsNotNull(objectResult);
        }

        /// <summary>
        /// Given you have an MyEdenService api with a blob
        /// When you call the update transcript function with an MyEdenService file that does not return a transcript
        /// Then it should return a not found result.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithNoTranscriptReturnsNotFound()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockEventGridMyEdenServiceCreatedRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService,
                out Mock<IEventGridSubscriberService> mockEventGridSubscriberService,
                out Mock<IMyEdenServiceTranscriptionService> mockMyEdenServiceTranscriptionService);

            MockEventGridSubscriberServiceDeconstructResponse(mockEventGridSubscriberService);

            mockMyEdenServiceTranscriptionService
                .Setup(s => s.GetMyEdenServiceTranscriptFromCognitiveServicesAsync(It.IsAny<System.IO.Stream>()))
                .ReturnsAsync((string)null);

            // act
            var response = await sut.UpdateTranscript(
                mockRequest.Object,
                mockLogger.Object).ConfigureAwait(false);
            var objectResult = (NotFoundResult)response;

            // assert
            Assert.IsNotNull(objectResult);
        }

        /// <summary>
        /// Given you have an MyEdenService api with a blob
        /// When you call the update transcript function to validate it handles the event grid subscription event
        /// Then it should return the action result return by the Event Subscription service.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithSubscriptionValidationEventReturnsActionResult()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockEventGridMyEdenServiceCreatedRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService,
                out Mock<IEventGridSubscriberService> mockEventGridSubscriberService,
                out Mock<IMyEdenServiceTranscriptionService> mockMyEdenServiceTranscriptionService);

            MockEventGridSubscriberServiceDeconstructResponse(mockEventGridSubscriberService);

            mockEventGridSubscriberService
                .Setup(s => s.HandleSubscriptionValidationEvent(It.IsAny<string>(), It.IsAny<StringValues>()))
                .Returns(new OkResult());

            // act
            var response = await sut.UpdateTranscript(
                mockRequest.Object,
                mockLogger.Object).ConfigureAwait(false);
            var objectResult = (OkResult)response;

            // assert
            Assert.IsNotNull(objectResult);
        }

        /// <summary>
        /// Given you have an MyEdenService api
        /// When you call the update transcription operation
        /// And a sub-component throws and exception
        /// Then it should log the exception and throw it.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithThrownExceptionThrowsException()
        {
            // arrange
            var fakeRepository = new FakeBlobRepository();
            Mock<AbstractLogger> mockLogger = new Mock<AbstractLogger>();
            var mockRequest = MyEdenServiceMockers.GetMockAddCompleteRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventPub,
                out Mock<IEventGridSubscriberService> mockEventSub,
                out Mock<IMyEdenServiceTranscriptionService> mockMyEdenServiceTranscriptService);

            var ex = new Exception("My error.");
            mockRequest.Setup(m => m.Headers)
                .Throws(ex);

            // act
            await Assert.ThrowsExceptionAsync<System.Exception>(() => sut.UpdateTranscript(mockRequest.Object, mockLogger.Object)).ConfigureAwait(false);

            mockLogger.Verify(moc => moc.Log(LogLevel.Error, It.IsAny<System.Exception>(), "Unhandled Exception."));
        }

        private static void MockEventGridSubscriberServiceDeconstructResponse(Mock<IEventGridSubscriberService> mockEventGridSubscriberService)
        {
            EventGridRequest<MyEdenServiceCreatedEventData> eventGridRequest = new EventGridRequest<MyEdenServiceCreatedEventData>()
            {
                UserId = Mockers.DefaultUserId,
                ItemId = Mockers.DefaultId,
                Event = new EventGridEvent<MyEdenServiceCreatedEventData>()
                {
                    EventTime = System.DateTime.Now,
                    EventType = "MyEdenServiceCreated",
                    Id = System.Guid.NewGuid().ToString(),
                    Subject = $"{Mockers.DefaultUserId}/{Mockers.DefaultId}",
                    Topic = "toco-events-topic",
                    Data = new MyEdenServiceCreatedEventData()
                    {
                        Category = "Test",
                    },
                },
            };

            mockEventGridSubscriberService
                .Setup(m => m.DeconstructEventGridMessage<MyEdenServiceCreatedEventData>(It.IsAny<string>()))
                .Returns(eventGridRequest);
        }
    }
}
