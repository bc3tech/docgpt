namespace DocGpt.Test
{
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis.Testing;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using VerifyCS = CSharpAnalyzerVerifier<DocGptAnalyzer>;

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
            string test = @"";

            await VerifyCS.VerifyAnalyzerAsync(test, DiagnosticResult.EmptyDiagnosticResults);
        }

        /// <summary>
        /// Analyzer the does not throw.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task AnalyzerPasses_DocumentedClass()
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
            string test = @"
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

            DiagnosticResult expected = VerifyCS.Diagnostic(DocGptAnalyzer.Rule).WithSpan(11, 15, 11, 22).WithArguments("ClassDeclaration", "MyClass");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }

        /// <summary>
        /// Test method2.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task AnalyzerThrows_ConstLiteralMember()
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
        internal class MyClass
        {   
            internal const string MyConst = ""Foo"";
        }
    }";

            DiagnosticResult expected = VerifyCS.Diagnostic(DocGptAnalyzer.Rule).WithSpan(14, 35, 14, 42).WithArguments("FieldDeclaration", "MyConst");
            await VerifyCS.VerifyAnalyzerAsync(test, expected);
        }
    }
}
