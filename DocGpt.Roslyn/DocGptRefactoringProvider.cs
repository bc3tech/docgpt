namespace DocGpt
{
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.CSharp;

    using System;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    /// The doc gpt code fix provider.
    /// </summary>
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(DocGptRefactoringProvider)), Shared]
    public class DocGptRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            Document document = context.Document;
            Microsoft.CodeAnalysis.Text.TextSpan textSpan = context.Span;
            CancellationToken cancellationToken = context.CancellationToken;

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);

            // Find the node at the selection.
            SyntaxNode node = root.FindNode(textSpan);
            if (!DocGptExecutor.SupportedSyntaxes.Contains(node.Kind()))
            {
                return;
            }

            // Register a code action that will invoke the fix.
            context.RegisterRefactoring(new DocGptCodeAction(context.Document, node.GetLocation()));
        }
    }
}
