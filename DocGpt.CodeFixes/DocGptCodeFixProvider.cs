namespace DocGpt
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using DocGpt.Options;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    /// <summary>
    /// The doc gpt code fix provider.
    /// </summary>
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DocGptCodeFixProvider)), Shared]
    public class DocGptCodeFixProvider : CodeFixProvider
    {
        /// <summary>
        /// Gets the fixable diagnostic ids.
        /// </summary>
        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get
            {
                return ImmutableArray.Create("CS1591",    // The diagnostic id of the XML Documentation an analyzer fired when XML doc gen is turned on but missing from a visible member
                DocGptAnalyzer.DiagnosticId);
            }
        }

        /// <summary>
        /// Get the fix all provider.
        /// </summary>
        /// <returns>A FixAllProvider.</returns>
        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        /// <summary>
        /// Registers the code fixes asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A Task.</returns>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            if (DocGptOptions.Instance.Endpoint is null
                || string.IsNullOrWhiteSpace(DocGptOptions.Instance.Endpoint.OriginalString))
            {
                return;
            }

            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            var diagnostic = context.Diagnostics.FirstOrDefault(i => this.FixableDiagnosticIds.Contains(i.Id));
            if (diagnostic is null)
            {
                return;
            }

            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<TypeDeclarationSyntax>().First();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(CodeAction.Create(
                title: CodeFixResources.CodeFixTitle,
                createChangedDocument: c => AddXmlDocumentationViaGptAsync(context.Document, declaration, c),
                equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
            diagnostic);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private async Task<Document> AddXmlDocumentationViaGptAsync(Document document, TypeDeclarationSyntax typeDecl, CancellationToken cancellationToken)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var options = DocGptOptions.Instance;
            var client = DocGptOptions.Instance.GetClient();

            return await Task.FromResult(document);
        }
    }
}
