namespace DocGpt;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.Formatting;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Document = Microsoft.CodeAnalysis.Document;

/// <summary>
/// This is an internal implementation of CodeAction, that uses OpenAI technology for generating summary text for a member's definition.
/// Preview and Change operations are overridden to add XML documentation to a document at a specified location. This happens asynchronously.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="DocGptCodeAction"/> class.
/// </remarks>
/// <param name="doc">The document associated with the action.</param>
/// <param name="location">The location within the document where the action is applied.</param>
internal class DocGptCodeAction(Document doc, Location location) : CodeAction
{

    /// <inheritdoc />
    public override string EquivalenceKey { get; } = nameof(CodeFixResources.CodeFixTitle);

    /// <inheritdoc />
    public override string Title { get; } = CodeFixResources.CodeFixTitle;

    /// <inheritdoc />
    protected override async Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken)
    {
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = location.SourceSpan;
        SyntaxNode? root = await doc.GetSyntaxRootAsync(cancellationToken);
        SyntaxNode? node = root?.FindNode(diagnosticSpan);

        return node is not null && DocGptExecutor.NodeTriggersGpt(node)
            ? DocGptCodeActionPreviewOperation.InstanceArray
            : await base.ComputePreviewOperationsAsync(cancellationToken);
    }

    /// <inheritdoc />
    protected override async Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken)
    {
        // Find the node at the diagnostic span
        Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = location.SourceSpan;
        SyntaxNode? root = await doc.GetSyntaxRootAsync(cancellationToken);
        SyntaxNode? node = root?.FindNode(diagnosticSpan);

        if (node is not null)
        {
            (SyntaxNode newNode, SyntaxNode nodeToReplace) = await DocGptExecutor.AddXmlDocumentationAsync(node, cancellationToken);

            SyntaxNode? newRoot = root?.ReplaceNode(nodeToReplace, newNode);

            // return a document with the new syntax root
            return newRoot is null ? doc : doc.WithSyntaxRoot(Formatter.Format(newRoot, doc.Project.Solution.Workspace));
        }

        return doc;
    }

    /// <summary>
    /// Represents an operation that sends the entire member's definition to the defined OpenAI endpoint for summary text generation and applies the result.
    /// This operation provides a preview of such operation in its title property. The preview can be asynchronously obtained via GetPreviewAsync method.
    /// </summary>
    private class DocGptCodeActionPreviewOperation : PreviewOperation
    {
        public static readonly DocGptCodeActionPreviewOperation Instance = new();
        public static readonly IEnumerable<CodeActionOperation> InstanceArray = [Instance];

        /// <summary>
        /// Initializes a new instance of the DocGptCodeActionPreviewOperation class.
        /// </summary>
        private DocGptCodeActionPreviewOperation() { }

        /// <inheritdoc />
        public override string Title { get; } = "Sends this entire member's definition (and body) to the defined OpenAI endpoint for summary text generation and applies the result.";

        /// <inheritdoc />
        public override Task<object?> GetPreviewAsync(CancellationToken cancellationToken) => Task.FromResult<object?>(this.Title);
    }
}
