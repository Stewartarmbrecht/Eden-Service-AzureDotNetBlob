namespace ContentReactor.MyEdenService.Service
{
    using System.IO;
    using System.Threading.Tasks;

    /// <summary>
    /// Submits an MyEdenService blob to the Cognitive Services Speech API to have it transcribed.
    /// </summary>
    public interface IMyEdenServiceTranscriptionService
    {
        /// <summary>
        /// Submits an MyEdenService blob to the Cognitive Services Speech API to have it transcribed.
        /// </summary>
        /// <param name="MyEdenServiceBlobStream">The MyEdenService file blob stream to translate.</param>
        /// <returns>The MyEdenService transcription.</returns>
        Task<string> GetMyEdenServiceTranscriptFromCognitiveServicesAsync(Stream MyEdenServiceBlobStream);
    }
}