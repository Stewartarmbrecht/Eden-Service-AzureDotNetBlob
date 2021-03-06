﻿namespace MyEdenSolution.MyEdenService.Service.Tests.Unit
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using MyEdenSolution.MyEdenService.Service;
    using MyEdenSolution.Common.Events;
    using MyEdenSolution.Common.Events.MyEdenService;
    using MyEdenSolution.Common.Fakes;
    using MyEdenSolution.Common.UserAuthentication;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Logging;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;

    /// <summary>
    /// Contains unit tests for the MyEdenService Service add opertions.
    /// </summary>
    [TestClass]
    public class AddCompleteFunctionTests
    {
        /// <summary>
        /// Given you have an MyEdenService api with an MyEdenService file started for upload
        /// When you call the operation to complete the add
        /// Then it should return a 204 NoContentResult.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithSuccessReturnsNoContentResult()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockAddCompleteRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(out FakeBlobRepository fakeBlobRepo);

            // act
            var response = await sut.AddComplete(mockRequest.Object, mockLogger.Object, Mockers.DefaultId).ConfigureAwait(false);
            var objectResult = (NoContentResult)response;

            // assert
            Assert.IsNotNull(objectResult);
        }

        /// <summary>
        /// Given you have an MyEdenService api with an MyEdenService file started for upload
        /// When you call the add complete operation
        /// Then it should update the cateogery and user id properties of the blob.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithSuccessUpdatesBlobMetadata()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockAddCompleteRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(out FakeBlobRepository fakeBlobRepo);

            // act
            await sut.AddComplete(mockRequest.Object, mockLogger.Object, Mockers.DefaultId).ConfigureAwait(false);

            // assert
            Assert.AreEqual(Mockers.DefaultCategoryName, fakeBlobRepo.Blobs.Single().Properties[Mockers.CategoryIdMetadataName]);
            Assert.AreEqual(Mockers.DefaultUserId, fakeBlobRepo.Blobs.Single().Properties[Mockers.UserIdMetadataName]);
        }

        /// <summary>
        /// Given you have an MyEdenService service with an MyEdenService file started for upload
        /// When you call the AddCompleteFunctionMyEdenServiceNoteAsync method
        /// Then it should raise an MyEdenServiceCreated event.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithSuccessPublishesMyEdenServiceCreatedEventToEventGrid()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockAddCompleteRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out FakeBlobRepository fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService);

            // act
            await sut.AddComplete(mockRequest.Object, mockLogger.Object, Mockers.DefaultId).ConfigureAwait(false);

            // assert
            mockEventGridPublisherService.Verify(
                m => m.PostEventGridEventAsync(
                    MyEdenServiceEvents.MyEdenServiceCreated,
                    $"{Mockers.DefaultUserId}/{Mockers.DefaultId}",
                    It.Is<MyEdenServiceCreatedEventData>(d => d.Category == Mockers.DefaultCategoryName)),
                Times.Once);
        }

        /// <summary>
        /// Given you have an MyEdenService api with no MyEdenService files uploaded
        /// When you call the add complete operation
        /// Then it should return an bad request with the error "MyEdenService has not yet been uploaded.".
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithMissingMyEdenServiceFileReturnsMyEdenServiceNotUploaded()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockAddCompleteRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out FakeBlobRepository fakeBlobRepo);

            fakeBlobRepo.Blobs.Clear();

            // act
            var response = await sut.AddComplete(mockRequest.Object, mockLogger.Object, Mockers.DefaultId).ConfigureAwait(false);
            var responseType = (BadRequestObjectResult)response;
            dynamic responseObject = responseType.Value;

            // assert
            Assert.IsNotNull(responseType);
            Assert.AreEqual("MyEdenService has not yet been uploaded.", responseObject.error);
        }

        /// <summary>
        /// Given you have an MyEdenService api with an MyEdenService file that is already processed
        /// When you call the add complete operation
        /// Then it should return an bad request response with an error stating "Image has already been created.".
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithMyEdenServiceFileAlreadyCreatedReturnsMyEdenServiceAlreadyCreated()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockAddCompleteRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out FakeBlobRepository fakeBlobRepo);

            fakeBlobRepo.Blobs[0].Properties[Mockers.CategoryIdMetadataName] = Mockers.DefaultCategoryName;

            // act
            var response = await sut.AddComplete(mockRequest.Object, mockLogger.Object, Mockers.DefaultId).ConfigureAwait(false);
            var responseType = (BadRequestObjectResult)response;

            // assert
            Assert.IsNotNull(responseType);
            Assert.AreEqual("Image has already been created.", ((dynamic)responseType.Value).error);
        }

        /// <summary>
        /// Given you have an MyEdenService apie with an MyEdenService file that is not processed
        /// When you call the complete add operation with the wrong user id
        /// Then it should return an bad request with the error "MyEdenService has not yet been uploaded.".
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithIncorrectUserIdReturnsMyEdenServiceNotUploaded()
        {
            // arrange
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockAddCompleteRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out FakeBlobRepository fakeBlobRepo);

            fakeBlobRepo.Blobs.Clear();
            fakeBlobRepo.AddFakeBlob(Mockers.MyEdenServiceContainerName, $"invaliduserid/{Mockers.DefaultId}");

            // act
            var response = await sut.AddComplete(mockRequest.Object, mockLogger.Object, Mockers.DefaultId).ConfigureAwait(false);
            var responseType = (BadRequestObjectResult)response;

            // assert
            Assert.IsNotNull(responseType);
            Assert.AreEqual("MyEdenService has not yet been uploaded.", ((dynamic)responseType.Value).error);
        }

        /// <summary>
        /// Given you have an MyEdenService api
        /// When you call the begin add operation without a user id
        /// Then it should return a bad request with the error returned by the user authentication service.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithMissingUserIdReturnsBadRequest()
        {
            // arrange
            string userId;
            var fakeRepository = new FakeBlobRepository();
            Mock<ILogger> mockLogger = new Mock<ILogger>();
            var mockRequest = MyEdenServiceMockers.GetMockAddCompleteRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService);

            IActionResult actionResult = new BadRequestObjectResult(new { error = "Error." });
            mockUserAuth.Setup(m => m.GetUserIdAsync(It.IsAny<HttpRequest>(), out userId, out actionResult))
                .Returns(Task.FromResult(false));

            // act
            var response = await sut.AddComplete(mockRequest.Object, mockLogger.Object, Mockers.DefaultId).ConfigureAwait(false);
            var objectResult = (BadRequestObjectResult)response;
            var addResponse = (dynamic)objectResult.Value;

            // assert
            Assert.AreEqual("Error.", addResponse.error);
        }

        /// <summary>
        /// Given you have an MyEdenService api
        /// When you call the begin add operation
        /// And a sub-component throws and exception
        /// Then it should log the exception and throw it.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1303", Justification="Reviewed")]
        [TestMethod]
        public async Task WithThrownExceptionThrowsException()
        {
            // arrange
            string userId;
            var fakeRepository = new FakeBlobRepository();
            Mock<AbstractLogger> mockLogger = new Mock<AbstractLogger>();
            var mockRequest = MyEdenServiceMockers.GetMockAddCompleteRequest();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeBlobRepo,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService);

            IActionResult actionResult = new BadRequestObjectResult(new { error = "Error." });
            System.Exception ex = new System.Exception("My error.");
            mockUserAuth.Setup(m => m.GetUserIdAsync(It.IsAny<HttpRequest>(), out userId, out actionResult))
                .ThrowsAsync(ex);

            // act
            await Assert.ThrowsExceptionAsync<System.Exception>(() => sut.AddComplete(mockRequest.Object, mockLogger.Object, Mockers.DefaultId)).ConfigureAwait(false);

            mockLogger.Verify(moc => moc.Log(LogLevel.Error, It.IsAny<System.Exception>(), "Unhandled Exception."));
        }

        /// <summary>
        /// Given you have an MyEdenService api
        /// When you call the complete add operation
        /// And the request has invalid json
        /// Then it should return a BadRequestObjectResult
        /// And the object should have an error property with the string 'Body should be provided in JSON format.'.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithInvalidJsonRequestReturnsBadRequest()
        {
            // arrange
            Mock<AbstractLogger> mockLogger = new Mock<AbstractLogger>();
            var mockRequest = Mockers.MockRequestWithInvalidJson();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeRepository,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService);

            // act
            var response = await sut.AddComplete(mockRequest.Object, mockLogger.Object, Mockers.DefaultId).ConfigureAwait(false);
            var responseType = (BadRequestObjectResult)response;

            Assert.IsNotNull(responseType);
            Assert.AreEqual("Body should be provided in JSON format.", ((dynamic)responseType.Value).error);
        }

        /// <summary>
        /// Given you have an MyEdenService api
        /// When you call the complete add operation
        /// And the request has no payload
        /// Then it should return a BadRequestObjectResult
        /// And the object should have an error property with the string 'Missing required property 'categoryId'.'.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithNullPayloadRequestReturnsBadRequest()
        {
            // arrange
            Mock<AbstractLogger> mockLogger = new Mock<AbstractLogger>();
            var mockRequest = Mockers.MockRequestWithNoPayload();
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeRepository,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService);

            // act
            var response = await sut.AddComplete(mockRequest.Object, mockLogger.Object, Mockers.DefaultId).ConfigureAwait(false);
            var responseType = (BadRequestObjectResult)response;

            Assert.IsNotNull(responseType);
            Assert.AreEqual("Missing required property 'categoryId'.", ((dynamic)responseType.Value).error);
        }

        /// <summary>
        /// Given you have an MyEdenService api
        /// When you call the complete add operation
        /// And the request has no payload
        /// Then it should return a BadRequestObjectResult
        /// And the object should have an error property with the string 'Missing required property 'categoryId'.'.
        /// </summary>
        /// <returns>Task for running the test.</returns>
        [TestMethod]
        public async Task WithMissingCategoryIdReturnsBadRequest()
        {
            // arrange
            Mock<AbstractLogger> mockLogger = new Mock<AbstractLogger>();
            var mockRequest = Mockers.MockRequest(new { cateogryId = string.Empty });
            var sut = MyEdenServiceMockers.GetApiFunctionsWithBlobUploaded(
                out Mock<IUserAuthenticationService> mockUserAuth,
                out FakeBlobRepository fakeRepository,
                out Mock<IEventGridPublisherService> mockEventGridPublisherService);

            // act
            var response = await sut.AddComplete(mockRequest.Object, mockLogger.Object, Mockers.DefaultId).ConfigureAwait(false);
            var responseType = (BadRequestObjectResult)response;

            Assert.IsNotNull(responseType);
            Assert.AreEqual("Missing required property 'categoryId'.", ((dynamic)responseType.Value).error);
        }
    }
}
