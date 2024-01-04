namespace DocGpt.Test
{
    using System.Threading.Tasks;

    using Microsoft.VisualStudio.TestTools.UnitTesting;

    using VerifyCS = CSharpCodeFixVerifier<DocGptAnalyzer, DocGptCodeFixProvider>;

    /// <summary>
    /// The doc gpt unit test.
    /// </summary>
    [TestClass]
    public class CodeFixTests
    {
        /// <summary>
        /// Analyzers the throws class decl.
        /// </summary>
        /// <returns>A Task.</returns>
        [TestMethod]
        public async Task CodeFix_DGPT001()
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

            var fixd = @"
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

            var expected = VerifyCS.Diagnostic(DocGptAnalyzer.Rule).WithSpan(11, 15, 11, 22).WithArguments("MyClass");
            await VerifyCS.VerifyCodeFixAsync(test, expected, fixd);
        }
    }
}
