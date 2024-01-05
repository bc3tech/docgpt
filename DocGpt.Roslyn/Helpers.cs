namespace DocGpt
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;

    internal static class Helpers
    {
        public static bool HasOverrideModifier(SyntaxNode node) => node is MemberDeclarationSyntax m ? m.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.OverrideKeyword)) : false;

        public static async Task<Document> DecorateWithInheritDocAsync(SyntaxNode node, Document document, CancellationToken cancellationToken = default)
        {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);

            // If we're overriding, return the node with an inheritdoc tag
            SyntaxTriviaList commentTrivia = SyntaxFactory.ParseLeadingTrivia(@"/// <inheritdoc />
").InsertRange(0, node.GetLeadingTrivia());
            SyntaxNode newRoot = root.ReplaceNode(node, node.WithLeadingTrivia(commentTrivia));
            return document.WithSyntaxRoot(Formatter.Format(newRoot, document.Project.Solution.Workspace));
        }
    }
}
