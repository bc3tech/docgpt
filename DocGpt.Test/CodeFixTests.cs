namespace DocGpt.Test
{
    using DocGpt.Options;

    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using System;
    using System.Threading.Tasks;

    using VerifyCS = CSharpCodeFixVerifier<DocGptAnalyzer, DocGptCodeFixProvider>;

    /// <summary>
    /// The doc gpt unit test.
    /// </summary>
    [TestClass]
    public class CodeFixTests
    {
        public TestContext TestContext { get; set; }

        [AssemblyInitialize]
        public static void AsmInit(TestContext _)
        {
            DocGptOptions.Instance.Endpoint = new Uri("http://localhost:5000");
            DocGptOptions.Instance.ApiKey = "foo";
            DocGptOptions.Instance.ModelDeploymentName = "foo";
        }

        private const string ApiKey = "foo";

        /// <summary>
        /// Analyzers the throws class decl.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task CodeFix_DGPT001_Constant_UseValue()
        {
            DocGptOptions.Instance.UseValueForLiteralConstants = true;

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

            DiagnosticResult expected = VerifyCS.Diagnostic(DocGptAnalyzer.Rule).WithSpan(14, 31, 14, 38).WithArguments("FieldDeclaration", "MyConst");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixd);
        }

        /// <summary>
        /// Analyzers the throws class decl.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task CodeFix_DGPT001_Constant_DoNotUseValue()
        {
            DocGptOptions.Instance.UseValueForLiteralConstants = false;

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
        internal const string MyConst = ""Foo"";
    }
}";

            DiagnosticResult expected = VerifyCS.Diagnostic(DocGptAnalyzer.Rule).WithSpan(14, 31, 14, 38).WithArguments("FieldDeclaration", "MyConst");
            try
            {
                await VerifyCS.VerifyCodeFixAsync(test, expected, fixd);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType<Azure.RequestFailedException>(e);
            }
        }

        /// <summary>
        /// Analyzers the throws class decl.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task CodeFix_DGPT001_Override_UseInheritDoc()
        {
            DocGptOptions.Instance.OverridesBehavior = OverrideBehavior.UseInheritDoc;
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
        /// <summary>Foo</summary>
        protected virtual bool DoSomething() => true;
    }

    /// <summary></summary>
    class MyDerivedClass : MyClass
    {
        protected override bool DoSomething() => false;
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
        protected virtual bool DoSomething() => true;
    }

    /// <summary></summary>
    class MyDerivedClass : MyClass
    {
        /// <inheritdoc />
        protected override bool DoSomething() => false;
    }
}";

            DiagnosticResult expected = VerifyCS.Diagnostic(DocGptAnalyzer.Rule).WithSpan(21, 33, 21, 44).WithArguments("MethodDeclaration", "DoSomething");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixd);
        }

        /// <summary>
        /// Analyzers the throws class decl.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task CodeFix_DGPT001_Override_DoNotDocument()
        {
            DocGptOptions.Instance.OverridesBehavior = OverrideBehavior.DoNotDocument;

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
        /// <summary>Foo</summary>
        protected virtual bool DoSomething() => true;
    }

    /// <summary></summary>
    class MyDerivedClass : MyClass
    {
        protected override bool DoSomething() => false;
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
        protected virtual bool DoSomething() => true;
    }

    /// <summary></summary>
    class MyDerivedClass : MyClass
    {
        protected override bool DoSomething() => false;
    }
}";

            await VerifyCS.VerifyCodeFixAsync(test, DiagnosticResult.EmptyDiagnosticResults, fixd);
        }

        /// <summary>
        /// Analyzers the throws class decl.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task CodeFix_DGPT001_Override_UseGpt()
        {
            DocGptOptions.Instance.OverridesBehavior = OverrideBehavior.GptSummarize;

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
        /// <summary>Foo</summary>
        protected virtual bool DoSomething() => true;
    }

    /// <summary></summary>
    class MyDerivedClass : MyClass
    {
        protected override bool DoSomething() => false;
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
        protected virtual bool DoSomething() => true;
    }

    /// <summary></summary>
    class MyDerivedClass : MyClass
    {
        protected override bool DoSomething() => false;
    }
}";

            DiagnosticResult expected = VerifyCS.Diagnostic(DocGptAnalyzer.Rule).WithSpan(21, 33, 21, 44).WithArguments("MethodDeclaration", "DoSomething");
            try
            {
                await VerifyCS.VerifyCodeFixAsync(test, expected, fixd);
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType<Azure.RequestFailedException>(e);
            }
        }
    }
}
