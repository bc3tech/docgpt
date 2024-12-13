namespace DocGpt.Test;
using DocGpt.Options;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Text.RegularExpressions;
using System.Threading.Tasks;

using VerifyCS = CSharpCodeFixVerifier<DocGptAnalyzer, DocGptCodeFixProvider>;

/// <summary>
/// The doc gpt unit test.
/// </summary>
[TestClass]
public partial class CodeFixTests
{
    public required TestContext TestContext { get; set; }

    /// <summary>
    /// Analyzers the throws class decl.
    /// </summary>
    /// <returns>A Task.</returns>
    [TestMethod]
    public async Task CodeFix_DGPT001_Constant_UseValue()
    {
        DocGptOptions.Instance.UseValueForLiteralConstants = true;

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
        internal const string MyConst = ""Foo"";
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
    [Ignore("Manual activation required; needs live API execution")]
    public async Task CodeFix_DGPT001_Constant_DoNotUseValue()
    {
        DocGptOptions.Instance.UseValueForLiteralConstants = false;

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
        internal const string MyConst = ""Foo"";
    }
}";

        DiagnosticResult expected = VerifyCS.Diagnostic(DocGptAnalyzer.Rule).WithSpan(14, 31, 14, 38).WithArguments("FieldDeclaration", "MyConst");
        await VerifyCS.VerifyGptDidSomethingAsync(expected, test);
    }

    /// <summary>
    /// Analyzers the throws class decl.
    /// </summary>
    /// <returns>A Task.</returns>
    [TestMethod]
    public async Task CodeFix_DGPT001_Override_UseInheritDoc()
    {
        DocGptOptions.Instance.OverridesBehavior = OverrideBehavior.UseInheritDoc;
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
        /// <summary>Foo</summary>
        protected virtual bool DoSomething() => true;
    }

    /// <summary></summary>
    class MyDerivedClass : MyClass
    {
        protected override bool DoSomething() => false;
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
        /// <summary>Foo</summary>
        protected virtual bool DoSomething() => true;
    }

    /// <summary></summary>
    class MyDerivedClass : MyClass
    {
        protected override bool DoSomething() => false;
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
    [Ignore("Manual activation required; needs live API execution")]
    public async Task CodeFix_DGPT001_Override_UseGpt()
    {
        DocGptOptions.Instance.OverridesBehavior = OverrideBehavior.GptSummarize;

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
        await VerifyCS.VerifyGptDidSomethingAsync(expected, test);
    }

    /// <summary>
    /// https://github.com/bc3tech/docgpt/issues/2
    /// </summary>
    [TestMethod]
    [Ignore("Manual activation required; needs live API execution")]
    public async Task Issue_2_Conflicting_Line_Endings()
    {
        DocGptOptions.Instance.OverridesBehavior = OverrideBehavior.GptSummarize;

        var test = @"
        protected override bool DoSomething() => false;
";

        SyntaxTree tree = CSharpSyntaxTree.ParseText(test);
        Microsoft.CodeAnalysis.CSharp.Syntax.CompilationUnitSyntax testNode = tree.GetCompilationUnitRoot();
        (SyntaxNode newNode, SyntaxNode _) = await DocGptExecutor.AddXmlDocumentationAsync(testNode, cancellationToken: default);

        var newText = newNode.GetText().ToString();

        Assert.AreEqual(0, LineFeedRegex().Matches(newText).Count, "There should be no LineFeed-only lines in the output, as there were none in the input. Output:\r\n{0}", newText);

        test = "\nprotected override bool DoSomething() => false;\n";

        tree = CSharpSyntaxTree.ParseText(test);
        testNode = tree.GetCompilationUnitRoot();
        (newNode, _) = await DocGptExecutor.AddXmlDocumentationAsync(testNode, cancellationToken: default);

        newText = newNode.GetText().ToString();

        Assert.AreEqual(0, CrLfRegex().Matches(newText).Count, "There should be no CrLf lines in the output, as there were none in the input. Output:\r\n{0}", newText);
    }

    [GeneratedRegex("(?<!\r)\n", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex LineFeedRegex();

    [GeneratedRegex("\r\n$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant)]
    private static partial Regex CrLfRegex();
}
