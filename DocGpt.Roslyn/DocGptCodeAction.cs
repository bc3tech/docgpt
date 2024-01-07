namespace DocGpt
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;

    using static Helpers;

    /// <summary>
    /// This is an internal implementaion of CodeAction, that uses OpenAI technology for generating summary text for a member's definition.
    /// Preview and Change operations are overridden to add XML documentation to a document at a specified location. This happens asynchronously.
    /// </summary>
    internal class DocGptCodeAction : CodeAction
    {
        private readonly Document _doc;
        private readonly Location _location;

        /// <summary>
        /// Initializes a new instance of the <see cref="DocGptCodeAction"/> class.
        /// </summary>
        /// <param name="doc">The document associated with the action.</param>
        /// <param name="location">The location within the document where the action is applied.</param>
        public DocGptCodeAction(Document doc, Location location)
        {
            _doc = doc;
            _location = location;
        }

        /// <inheritdoc />
        public override string EquivalenceKey { get; } = nameof(CodeFixResources.CodeFixTitle);

        /// <inheritdoc />
        public override string Title { get; } = CodeFixResources.CodeFixTitle;

        /// <inheritdoc />
        protected override async Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken)
        {
            Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = _location.SourceSpan;
            SyntaxNode root = await _doc.GetSyntaxRootAsync(cancellationToken);
            SyntaxNode node = root.FindNode(diagnosticSpan);

            if (HasOverrideModifier(node) || IsConstantLiteral(ref node))
            {
                return await base.ComputePreviewOperationsAsync(cancellationToken);

            }

            return DocGptCodeActionPreviewOperation.InstanceArray;
        }

        /// <inheritdoc />
        protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken) => DocGptExecutor.AddXmlDocumentationAsync(_doc, _location, cancellationToken);

        /// <summary>
        /// Represents an operation that sends the entire member's definition to the defined OpenAI endpoint for summary text generation and applies the result.
        /// This operation provides a preview of such operation in its title property. The preview can be asynchronously obtained via GetPreviewAsync method.
        /// </summary>
        private class DocGptCodeActionPreviewOperation : PreviewOperation
        {
            public static readonly DocGptCodeActionPreviewOperation Instance = new DocGptCodeActionPreviewOperation();
            public static readonly IEnumerable<CodeActionOperation> InstanceArray = new CodeActionOperation[] { Instance };

            /// <summary>
            /// Initializes a new instance of the DocGptCodeActionPreviewOperation class.
            /// </summary>
            private DocGptCodeActionPreviewOperation() { }

            /// <inheritdoc />
            public override string Title { get; } = "Sends this entire member's definition (and body) to the defined OpenAI endpoint for summary text generation and applies the result.";

            /// <inheritdoc />
            public override Task<object> GetPreviewAsync(CancellationToken cancellationToken) => Task.FromResult<object>(Title);
        }
    }
}
