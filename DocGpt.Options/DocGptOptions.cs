namespace DocGpt.Options
{
    using System;
    using System.ComponentModel;

    /// <summary>
    /// The page.
    /// </summary>
    public class DocGptOptions
    {
        public static readonly DocGptOptions Instance = new DocGptOptions();
        protected DocGptOptions() { }

        /// <summary>
        /// Gets or Sets the endpoint.
        /// </summary>
        [Category("OpenAI Configuration")]
        [DisplayName("Endpoint URL")]
        [Description("The OpenAI or Azure OpenAI endpoint url.")]
        public Uri Endpoint { get; set; }

        /// <summary>
        /// Gets or Sets the api key.
        /// </summary>
        [Category("OpenAI Configuration")]
        [DisplayName("API Key")]
        [Description("The OpenAI or Azure OpenAI API Key.")]
        public string ApiKey { get; set; }

        /// <summary>
        /// Gets or Sets the model deployment name.
        /// </summary>
        [Category("OpenAI Configuration")]
        [DisplayName("Model/Deployment Name")]
        [Description("The OpenAI or Azure OpenAI API model or deployment name.")]
        public string ModelDeploymentName { get; set; }
    }
}
