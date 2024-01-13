namespace DocGpt.Vsix
{
    using DocGpt.Options;

    using Microsoft.VisualStudio.Shell;

    using System;
    using System.ComponentModel;
    using System.Runtime.InteropServices;

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
        public string Endpoint { get => _options.Endpoint?.OriginalString; set => _options.Endpoint = Uri.TryCreate(value, UriKind.Absolute, out var r) ? r : null; }

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

        /// <summary>
        /// Gets or Sets whether/not to add <inheritdoc /> tags to overridden members.
        /// </summary>
        [Category("Documentation Configuration")]
        [DisplayName("Add <inheritdoc /> to overridden members")]
        [Description("Adds the <inheritdoc /> to all members that override inherited members")]
        public bool UseInheritDocForOverrides { get => _options.UseInheritDocForOverrides; set => _options.UseInheritDocForOverrides = value; }

        /// <summary>
        /// Gets or Sets whether/not to add <summary>{value}</summary> on literal constants.
        /// </summary>
        [Category("Documentation Configuration")]
        [DisplayName("Use {value} for literal constants")]
        [Description("Adds <summary>{value}</summary> on literal constants, where {value} is the literal value assigned to the constant.")]
        public bool UseValueForLiteralConstants { get => _options.UseValueForLiteralConstants; set => _options.UseValueForLiteralConstants = value; }
    }
}
