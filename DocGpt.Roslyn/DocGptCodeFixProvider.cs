namespace DocGpt
{
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
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
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create("CS1591",    // The diagnostic id of the XML Documentation an analyzer fired when XML doc gen is turned on but missing from a visible member
                    "CD1606",   // Diagnostic from "CodeDocumentor" analyzer (https://marketplace.visualstudio.com/items?itemName=DanTurco.CodeDocumentor)
                DocGptAnalyzer.DiagnosticId);

        /// <summary>
        /// Get the fix all provider.
        /// </summary>
        /// <returns>A FixAllProvider.</returns>
        public sealed override FixAllProvider GetFixAllProvider() =>
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            WellKnownFixAllProviders.BatchFixer;

        /// <summary>
        /// Registers the code fixes asynchronously.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <returns>A Task.</returns>
        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            Diagnostic diagnostic = context.Diagnostics.FirstOrDefault(i => this.FixableDiagnosticIds.Contains(i.Id));
            if (diagnostic is null)
            {
                return;
            }

            Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            SyntaxNode node = root.FindNode(diagnosticSpan);
            if (node is VariableDeclaratorSyntax v)
            {
                if (node.Parent?.Parent is FieldDeclarationSyntax f)
                {
                    node = f;
                }
                else
                {
                    return;
                }
            }

            var code = root.GetText().GetSubText(diagnosticSpan).ToString();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(new DocGptCodeAction(context.Document, diagnostic.Location), diagnostic);
        }
    }
}
