namespace ContentReactor.MyEdenService.Service
{
    using System;
    using Newtonsoft.Json;

    /// <summary>
    /// Includes the Id and url to upload the new MyEdenService file to.
    /// </summary>
    [Serializable]
    public class AddCompleteRequest
    {
        /// <summary>
        /// Gets or sets the category for the MyEdenService file.
        /// </summary>
        /// <value>The string value of the category id.</value>
        [JsonProperty("categoryId")]
        public string CategoryId { get; set; }
    }
}
