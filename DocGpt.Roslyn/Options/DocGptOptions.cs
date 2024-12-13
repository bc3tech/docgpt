namespace DocGpt.Options;

using Azure.AI.OpenAI;

using OpenAI;
using OpenAI.Chat;

using System;
using System.ClientModel;

/// <summary>
/// Represents the configuration options for an OpenAI or Azure-based OpenAI service.
/// This includes properties required for connection such as the endpoint URL and API key.
/// It also includes the name of the model or deployment to use.
/// </summary>
public sealed class DocGptOptions
{
    public static readonly DocGptOptions Instance = new();
    /// <summary>
    /// Initializes a new instance of the <see cref="DocGptOptions"/> class.
    /// </summary>
    private DocGptOptions() { }

    /// <summary>
    /// Gets or Sets the endpoint.
    /// </summary>
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
    /// Gets or sets the OpenAI or Azure OpenAI API key. Changing the value of this property will reset the underlying client.
    /// </summary>
    /// <value>
    /// The API key.
    /// </value>
    public string ApiKey
    {
        get => _apiKey ?? string.Empty;
        set
        {
            _apiKey = value;
            _client = null;
        }
    }

    /// <summary>
    /// Gets or sets the OpenAI model or Azure OpenAI deployment name.
    /// </summary>
    /// <value>
    /// The OpenAI model or Azure OpenAI deployment name.
    /// </value>
    public string? ModelDeploymentName { get; set; }

    private ChatClient? _client;
    private string? _apiKey;
    private Uri _endpoint = new("https://api.openai.com");

    /// <summary>
    /// Gets an instance of the OpenAIClient. If the client instance is null, it creates a new instance 
    /// based on whether the host endpoint ends with "azure.com" or not, using the provided API key.
    /// </summary>
    public ChatClient GetClient()
    {
        if (string.IsNullOrWhiteSpace(this.ModelDeploymentName))
        {
            throw new ArgumentNullException(nameof(this.ModelDeploymentName));
        }

        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            throw new ArgumentNullException(nameof(this.ApiKey));
        }

        _client ??= _endpoint.Host.EndsWith("azure.com", StringComparison.OrdinalIgnoreCase)
            ? new AzureOpenAIClient(_endpoint ?? throw new ArgumentNullException(nameof(this.Endpoint)), new ApiKeyCredential(_apiKey!)).GetChatClient(this.ModelDeploymentName)
            : new OpenAIClient(_apiKey).GetChatClient(this.ModelDeploymentName);

        return _client;
    }

    /// <summary>
    /// Gets or Sets whether/not to add <inheritdoc /> tags to overridden members.
    /// </summary>
    public OverrideBehavior OverridesBehavior { get; set; } = OverrideBehavior.UseInheritDoc;

    /// <summary>
    /// Gets or Sets whether/not to add <summary>{value}</summary> on literal constants.
    /// </summary>
    public bool UseValueForLiteralConstants { get; set; } = true;
}
