namespace DocGpt
{
    using System.Collections.Immutable;
    using System.Linq;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;

    /// <summary>
    /// The doc gpt analyzer.
    /// </summary>
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DocGptAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "DGPT001";

        // You can change these strings in the AnalyzerResources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        // See https://github.com/dotnet/roslyn/blob/main/docs/analyzers/Localizing%20Analyzers.md for more on localization
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(AnalyzerResources.AnalyzerTitle), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(AnalyzerResources.AnalyzerMessageFormat), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(AnalyzerResources.AnalyzerDescription), AnalyzerResources.ResourceManager, typeof(AnalyzerResources));
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

            foreach (SyntaxKind k in DocGptExecutor.SupportedSyntaxes)
            {
                context.RegisterSyntaxNodeAction(AnalyzeNode, k);
            }
        }

        private static void AnalyzeNode(SyntaxNodeAnalysisContext context)
        {
            SyntaxNode node = context.Node;
            //if (context.Node is PropertyDeclarationSyntax s && IsAutoProperty(s))
            //{
            //    return;
            //}

            // Check if the node has leading trivia (which is where the XML documentation would be)
            if (node.HasLeadingTrivia)
            {
                // Go through each piece of trivia leading the node
                foreach (SyntaxTrivia trivia in node.GetLeadingTrivia())
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
            ISymbol symbol = context.SemanticModel.GetDeclaredSymbol(node);
            Diagnostic diagnostic = Diagnostic.Create(Rule, symbol.Locations[0], node.Kind(), symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private static bool IsAutoProperty(PropertyDeclarationSyntax property)
        {
            // An auto-property will have at least one accessor with no body
            return property.AccessorList != null &&
                   property.AccessorList.Accessors.Any(a => a.Body == null && a.ExpressionBody == null);
        }
    }
}
