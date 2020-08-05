namespace MyEdenSolution.MyEdenService.Service
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Provides meta data and access url to download an MyEdenService file.
    /// </summary>
    public class GetResponse
    {
        /// <summary>
        /// Gets or sets id of the MyEdenService file.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets url to download MyEdenService file.
        /// </summary>
        [JsonProperty("MyEdenServiceUrl")]
        public Uri MyEdenServiceUrl { get; set; }

        /// <summary>
        /// Gets or sets transcript of the MyEdenService file.
        /// </summary>
        [JsonProperty("transcript")]
        public string Transcript { get; set; }
    }
}
