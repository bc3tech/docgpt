namespace DocGpt
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;

    using static Helpers;

    internal class DocGptCodeAction : CodeAction
    {
        private readonly Document _doc;
        private readonly Location _location;

        public DocGptCodeAction(Document doc, Location location)
        {
            _doc = doc;
            _location = location;
        }

        public override string EquivalenceKey { get; } = nameof(CodeFixResources.CodeFixTitle);

        /// <inheritdoc />
        public override string Title { get; } = CodeFixResources.CodeFixTitle;

        protected override async Task<IEnumerable<CodeActionOperation>> ComputePreviewOperationsAsync(CancellationToken cancellationToken)
        {
            Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = _location.SourceSpan;
            SyntaxNode root = await _doc.GetSyntaxRootAsync(cancellationToken);
            SyntaxNode node = root.FindNode(diagnosticSpan);

            return !HasOverrideModifier(node)
                ? DocGptCodeActionPreviewOperation.InstanceArray
                : await base.ComputePreviewOperationsAsync(cancellationToken);
        }

        protected override Task<Document> GetChangedDocumentAsync(CancellationToken cancellationToken) => DocGptExecutor.AddXmlDocumentationViaGptAsync(_doc, _location, cancellationToken);

        private class DocGptCodeActionPreviewOperation : PreviewOperation
        {
            public static readonly DocGptCodeActionPreviewOperation Instance = new DocGptCodeActionPreviewOperation();
            public static readonly IEnumerable<CodeActionOperation> InstanceArray = new CodeActionOperation[] { Instance };

            private DocGptCodeActionPreviewOperation() { }
            public override string Title { get; } = "Sends this entire member's definition to the defined OpenAI endpoint for summary text generation and applies the result.";

            public override Task<object> GetPreviewAsync(CancellationToken cancellationToken) => Task.FromResult<object>(Title);
        }
    }
}
