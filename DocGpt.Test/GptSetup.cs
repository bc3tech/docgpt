namespace DocGpt.Test;

using Azure;

using DocGpt.Options;

using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Threading.Tasks;

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

public static partial class CSharpCodeFixVerifier<TAnalyzer, TCodeFix>
    where TAnalyzer : DiagnosticAnalyzer, new()
    where TCodeFix : CodeFixProvider, new()
{
    public static async Task VerifyGptDidSomethingAsync(DiagnosticResult diag, string test)
    {
        try
        {
            await VerifyCodeFixAsync(test, diag, test);

            Assert.Fail("Should have failed the CodeFix verifier");
        }
        catch (AssertFailedException e)
        {
            // We expect any other assertion message because GPT will have populated the doc comment with a value; this is non-deterministic, so we pass as long as input != output
            Assert.AreNotEqual("Should have failed the CodeFix verifier", e.Message);
        }
        catch (RequestFailedException e)
        {
            Assert.Fail("Did you forget to put your OpenAI Key into GptSetup.cs? {0}", e);
        }
    }
}
