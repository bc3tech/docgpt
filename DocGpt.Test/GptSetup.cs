namespace DocGpt.Test;

using DocGpt.Options;

using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
internal static class GptSetup
{
    [AssemblyInitialize]
    public static void AsmInit(TestContext _)
    {
        DocGptOptions.Instance.Endpoint = new("https://api.openai.com");
        DocGptOptions.Instance.ApiKey = "...";
        DocGptOptions.Instance.ModelDeploymentName = "gpt-3.5-turbo-16k";
    }
}
