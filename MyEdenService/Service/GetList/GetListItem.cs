namespace MyEdenSolution.MyEdenService.Service
{
    using Newtonsoft.Json;

    /// <summary>
    /// MyEdenService note metadata.
    /// </summary>
    public class GetListItem
    {
        /// <summary>
        /// Gets or sets id of the MyEdenService note.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets preview of the MyEdenService not transcription.
        /// </summary>
        [JsonProperty("preview")]
        public string Preview { get; set; }
    }
}
