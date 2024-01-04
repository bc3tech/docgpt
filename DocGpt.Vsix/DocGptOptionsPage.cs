namespace DocGpt.Vsix
{
    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

    using DocGpt.Options;

    using Microsoft.VisualStudio.Shell;

    /// <summary>
    /// The doc gpt options page.
    /// </summary>
    [Guid("D2FB7185-E45E-4761-AA8D-85A5CF91D803")]
    public class DocGptOptionsPage : DialogPage
    {
        public const string CategoryName = "Doc GPT";
        public const string PageName = "General";

        private readonly DocGptOptions _options = DocGptOptions.Instance;

        /// <summary>
        /// Gets or Sets the endpoint.
        /// </summary>
        [Category("OpenAI Configuration")]
        [DisplayName("Endpoint URL")]
        [Description("The OpenAI or Azure OpenAI endpoint url.")]
        public Uri Endpoint { get => _options.Endpoint; set => _options.Endpoint = value; }

        /// <summary>
        /// Gets or Sets the api key.
        /// </summary>
        [Category("OpenAI Configuration")]
        [DisplayName("API Key")]
        [Description("The OpenAI or Azure OpenAI API Key.")]
        public string ApiKey { get => _options.ApiKey; set => _options.ApiKey = value; }

        /// <summary>
        /// Gets or Sets the model deployment name.
        /// </summary>
        [Category("OpenAI Configuration")]
        [DisplayName("Model/Deployment Name")]
        [Description("The OpenAI or Azure OpenAI API model or deployment name.")]
        public string ModelDeploymentName { get => _options.ModelDeploymentName; set => _options.ModelDeploymentName = value; }

    }
}
