namespace MyEdenSolution.MyEdenService.Service
{
    using System;
    using System.IO;
    using System.Net;
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Transcribes MyEdenService blob files using the Microsoft Congnitive Services
    /// speech to text api.
    /// </summary>
    public class MyEdenServiceTranscriptionService : IMyEdenServiceTranscriptionService
    {
        private static readonly string CognitiveServicesSpeechApiEndpoint = Environment.GetEnvironmentVariable("CognitiveServicesSpeechApiEndpoint");
        private static readonly string CognitiveServicesSpeechApiKey = Environment.GetEnvironmentVariable("CognitiveServicesSpeechApiKey");

        /// <summary>
        /// Transcribes MyEdenService blob files using the Microsoft Congnitive Services
        /// speech to text api.
        /// </summary>
        /// <param name="MyEdenServiceBlobStream">The stream that feeds the MyEdenService blob file.</param>
        /// <returns>The transcription of the MyEdenService file.</returns>
        public async Task<string> GetMyEdenServiceTranscriptFromCognitiveServicesAsync(Stream MyEdenServiceBlobStream)
        {
            var request = CreateMyEdenServiceTranscriptRequest(MyEdenServiceBlobStream);

            var response = await request.GetResponseAsync().ConfigureAwait(false);
            using var stream = response.GetResponseStream();
            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            var responseString = reader.ReadToEnd();

            return ProcessMyEdenServiceTranscriptResponse(responseString);
        }

        /// <summary>
        /// Creates the api request to submit the MyEdenService file to the
        /// Microsoft Cognitive Services Speech api.
        /// </summary>
        /// <param name="MyEdenServiceBlobStream">MyEdenService file to submit to the api.</param>
        /// <returns>The http request that will post the file to the api.</returns>
        protected internal static HttpWebRequest CreateMyEdenServiceTranscriptRequest(Stream MyEdenServiceBlobStream)
        {
            if (MyEdenServiceBlobStream == null)
            {
                throw new ArgumentNullException(nameof(MyEdenServiceBlobStream));
            }

            var apiUri = new Uri(CognitiveServicesSpeechApiEndpoint);
            var request = (HttpWebRequest)WebRequest.Create(apiUri);
            request.SendChunked = true;
            request.Accept = "application/json";
            request.Method = "POST";
            request.ProtocolVersion = HttpVersion.Version11;
            request.ContentType = "MyEdenService/wav; codec=MyEdenService/pcm; samplerate=16000";
            request.Headers["Ocp-Apim-Subscription-Key"] = CognitiveServicesSpeechApiKey;

            if (MyEdenServiceBlobStream.CanSeek)
            {
                MyEdenServiceBlobStream.Position = 0;
            }

            // open a request stream and write 1024 byte chunks in the stream one at a time
            using (var requestStream = request.GetRequestStream())
            {
                // read 1024 raw bytes from the input MyEdenService file
                var buffer = new byte[checked((uint)Math.Min(1024, (int)MyEdenServiceBlobStream.Length))];
                int bytesRead;
                while ((bytesRead = MyEdenServiceBlobStream.Read(buffer, 0, buffer.Length)) != 0)
                {
                    requestStream.Write(buffer, 0, bytesRead);
                }

                requestStream.Flush();
            }

            return request;
        }

        /// <summary>
        /// Gets the transcription from the http response body.
        /// </summary>
        /// <param name="responseString">The string from the http response body.</param>
        /// <returns>The display value of the best match transcription for the MyEdenService file.</returns>
        protected internal static string ProcessMyEdenServiceTranscriptResponse(string responseString)
        {
            dynamic responseJson = JObject.Parse(responseString);
            if (responseJson.RecognitionStatus != "Success")
            {
                return string.Empty;
            }

            var matches = responseJson.NBest;
            var bestMatch = matches.First;
            return bestMatch.Display;
        }
    }
}
