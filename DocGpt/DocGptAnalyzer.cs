namespace DocGpt
{
    using System.Collections.Immutable;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// The doc gpt analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DocGptAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DGPT001";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Documentation";

        internal static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        /// <summary>
        /// Gets the supported diagnostics.
        /// </summary>
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

        /// <summary>
        /// TODO: Add Summary
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Initialize(AnalysisContext context)
        {
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            // TODO: Consider registering other actions that act on syntax instead of or in addition to symbols
            // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Analyzer%20Actions%20Semantics.md for more information
            context.RegisterSyntaxNodeAction(AnalyzeNode, SyntaxKind.ClassDeclaration);
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            var node = context.Node;

            // Check if the node has leading trivia (which is where the XML documentation would be)
            if (node.HasLeadingTrivia)
            {
                // Go through each piece of trivia leading the node
                foreach (var trivia in node.GetLeadingTrivia())
                {
                    // Check if the trivia is of kind 'DocumentationCommentTrivia'
                    if (trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
                        trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
                    {
                        // XML documentation exists, so return without reporting a diagnostic
                        return;
                    }
                }
            }

            // If this point is reached, then no XML documentation was found
            // Create and report the diagnostic for missing XML documentation
            var symbol = context.SemanticModel.GetDeclaredSymbol(node);
            var diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }
    }
}
