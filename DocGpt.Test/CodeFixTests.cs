namespace DocGpt.Test
{
    using System.Threading.Tasks;

    using DocGpt.Options;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using VerifyCS = CSharpCodeFixVerifier<DocGptAnalyzer, DocGptCodeFixProvider>;

    /// <summary>
    /// The doc gpt unit test.
    /// </summary>
    [TestClass]
    public class CodeFixTests
    {
        public static TestContext Context { get; private set; }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            DocGptOptions.Instance.Endpoint = new System.Uri("http://localhost:5000");
            DocGptOptions.Instance.ApiKey = "foo";
            DocGptOptions.Instance.ModelDeploymentName = "foo";

            Context = context;
        }

        /// <summary>
        /// Analyzers the throws class decl.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task CodeFix_DGPT001_Constant()
        {
            string test = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    /// <summary></summary>
    class MyClass
    {
        internal const string MyConst = ""Foo"";
    }
}";

            string fixd = @"
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ConsoleApplication1
{
    /// <summary></summary>
    class MyClass
    {
        /// <summary>Foo</summary>
        internal const string MyConst = ""Foo"";
    }
}";

            Microsoft.CodeAnalysis.Testing.DiagnosticResult expected = VerifyCS.Diagnostic(DocGptAnalyzer.Rule).WithSpan(14, 31, 14, 38).WithArguments("FieldDeclaration", "MyConst");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixd);
        }
    }
}
