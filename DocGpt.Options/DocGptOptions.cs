namespace DocGpt.Options
{
    using System;
    using System.ComponentModel;

    using Azure.AI.OpenAI;

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
        public Uri Endpoint
        {
            get => _endpoint;
            set
            {
                _endpoint = value;
                _client = null;
            }
        }

        /// <summary>
        /// Gets or Sets the api key.
        /// </summary>
        [Category("OpenAI Configuration")]
        [DisplayName("API Key")]
        [Description("The OpenAI or Azure OpenAI API Key.")]
        public string ApiKey
        {
            get => _apiKey;
            set
            {
                _apiKey = value;
                _client = null;
            }
        }

        /// <summary>
        /// Gets or Sets the model deployment name.
        /// </summary>
        [Category("OpenAI Configuration")]
        [DisplayName("Model/Deployment Name")]
        [Description("The OpenAI or Azure OpenAI API model or deployment name.")]
        public string ModelDeploymentName { get; set; }

        private OpenAIClient _client;
        private string _apiKey;
        private Uri _endpoint;

        /// <summary>
        /// Gets the client.
        /// </summary>
        /// <returns>An OpenAIClient.</returns>
        public OpenAIClient GetClient()
        {
            if (_client is null)
            {
                _client =
                    _endpoint.Host.EndsWith("azure.com", StringComparison.OrdinalIgnoreCase)
                    ? new OpenAIClient(_endpoint, new Azure.AzureKeyCredential(_apiKey))
                    : new OpenAIClient(_apiKey);
            }

            return _client;
        }
    }
}
