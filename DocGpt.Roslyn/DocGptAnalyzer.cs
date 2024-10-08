namespace DocGpt;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

using System.Collections.Immutable;

using static Helpers;

/// <summary>
/// Represents a diagnostic analyzer for the DocGptAnalyzer class.
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

    internal static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Info, isEnabledByDefault: true, description: Description);

    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        foreach (SyntaxKind k in DocGptExecutor.SupportedSyntaxes)
        {
            context.RegisterSyntaxNodeAction(AnalyzeNode, k);
        }
    }

    /// <summary>
    /// Analyzes the given syntax node.
    /// If XML documentation exists for the node, no diagnostic is reported.
    /// If no XML documentation is found, a diagnostic for missing XML documentation is created and reported.
    /// </summary>
    private void AnalyzeNode(SyntaxNodeAnalysisContext context)
    {
        SyntaxNode node = context.Node;

        if (ShouldSkip(node))
        {
            return;
        }

        // Create and report the diagnostic
        (Location loc, var name) = getLocationAndName();
        var diagnostic = Diagnostic.Create(Rule, loc, node.Kind(), name);
        context.ReportDiagnostic(diagnostic);

        (Location, string) getLocationAndName()
        {
            if (node is FieldDeclarationSyntax fs)
            {
                SyntaxToken vi = fs.Declaration.Variables.First().Identifier;
                return (vi.GetLocation(), vi.ValueText);
            }

            ISymbol symbol = context.SemanticModel.GetDeclaredSymbol(node);
            return (symbol.Locations[0], symbol.Name);
        }
    }
}
