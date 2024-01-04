namespace DocGpt.Test
{
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using VerifyCS = DocGpt.Test.CSharpAnalyzerVerifier<DocGpt.DocGptAnalyzer>;

    /// <summary>
    /// The doc gpt unit test.
    /// </summary>
    [TestClass]
    public class AnalyzerTests
    {
        /// <summary>
        /// Analyzer the does not throw.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task AnalyzerPasses_BlankFile()
        {
            var test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test, DiagnosticResult.EmptyDiagnosticResults);
        }

        /// <summary>
        /// Analyzer the does not throw.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task AnalyzerPasses_DocumentedClass()
        {
            var test = @"
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
        }
    }";

            await VerifyCS.VerifyAnalyzerAsync(test, DiagnosticResult.EmptyDiagnosticResults);
        }

        /// <summary>
        /// Test method2.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task AnalyzerThrows_ClassDecl()
        {
            var test = @"
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Diagnostics;

    namespace ConsoleApplication1
    {
        class MyClass
        {   
        }
    }";

            var expected = VerifyCS.Diagnostic(DocGptAnalyzer.Rule).WithSpan(11, 15, 11, 22).WithArguments("MyClass");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}
