namespace DocGpt
{
    using DocGpt.Options;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;

    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    internal static class Helpers
    {
        public static bool HasOverrideModifier(SyntaxNode node) => node is MemberDeclarationSyntax m ? m.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.OverrideKeyword)) : false;

        public static Task<Document> DecorateWithInheritDocAsync(SyntaxNode node, Document document, CancellationToken cancellationToken = default) => DecorateWithXmlDocumentationAsync(node, document, @"/// <inheritdoc />", cancellationToken);

        public static Task<Document> DecorateWithValueAsSummaryAsync(FieldDeclarationSyntax node, Document document, CancellationToken cancellationToken) => DecorateWithXmlDocumentationAsync(node, document, $"/// <summary>{node.Declaration.Variables[0].Initializer.Value.ChildTokens().First().ValueText}</summary>", cancellationToken);

        public static async Task<Document> DecorateWithXmlDocumentationAsync(SyntaxNode node, Document document, string documentationContent, CancellationToken cancellationToken = default)
        {
            SyntaxTriviaList commentTrivia = SyntaxFactory.ParseLeadingTrivia($@"{documentationContent}
").InsertRange(0, node.GetLeadingTrivia());

            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxNode newRoot = root.ReplaceNode(node, node.WithLeadingTrivia(commentTrivia));
            return document.WithSyntaxRoot(Formatter.Format(newRoot, document.Project.Solution.Workspace));
        }

        public static bool ShouldSkip(SyntaxNode node)
        {
            // Skip nodes that already have XML documentation
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
                        return true;
                    }
                }
            }

            // Skip overrides if user has elected not to document them
            if (IsOverriddenMember(node))
            {
                return DocGptOptions.Instance.OverridesBehavior is OverrideBehavior.DoNotDocument;
            }

            // Skip field declarations that are not constants with a literal expression,
            // or if user has set the option to not document these.
            if (IsConstantLiteral(node, out var parentField))
            {
                return !DocGptOptions.Instance.UseValueForLiteralConstants;
            }

            return node is VariableDeclarationSyntax;
        }

        public static bool IsOverriddenMember(SyntaxNode node) => node is MemberDeclarationSyntax m ? m.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.OverrideKeyword)) : false;

        public static bool IsConstantLiteral(SyntaxNode node, out FieldDeclarationSyntax parentField)
        {
            parentField = null;

            if (node is VariableDeclaratorSyntax
                && node.Parent?.Parent is FieldDeclarationSyntax field)
            {
                parentField = field;
                if (field?.Modifiers.Any(SyntaxKind.ConstKeyword) is true
                    && field.Declaration.Variables.FirstOrDefault()?.Initializer?.Value is LiteralExpressionSyntax)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
