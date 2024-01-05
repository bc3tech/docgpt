namespace DocGpt
{
    using System;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DocGpt.Options;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeRefactorings;
    using Microsoft.CodeAnalysis.CSharp;

    /// <summary>
    /// The doc gpt code fix provider.
    /// </summary>
    [ExportCodeRefactoringProvider(LanguageNames.CSharp, Name = nameof(DocGptRefactoringProvider)), Shared]
    public class DocGptRefactoringProvider : CodeRefactoringProvider
    {
        public override async Task ComputeRefactoringsAsync(CodeRefactoringContext context)
        {
            if (DocGptOptions.Instance.Endpoint is null
                || string.IsNullOrWhiteSpace(DocGptOptions.Instance.Endpoint.OriginalString))
            {
                return;
            }

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
