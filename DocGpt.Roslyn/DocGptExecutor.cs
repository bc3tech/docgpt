namespace DocGpt
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    using Azure.AI.OpenAI;

    using DocGpt.Options;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Formatting;

    using static Helpers;

    /// <summary>
    /// An internal static class which hosts methods to add XML documentation to a provided Document using GPT service.
    /// </summary>
    internal static class DocGptExecutor
    {
        public static readonly SyntaxKind[] SupportedSyntaxes = new[] {
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.EnumDeclaration,
                SyntaxKind.DelegateDeclaration,
                SyntaxKind.FieldDeclaration,
                SyntaxKind.PropertyDeclaration,
                SyntaxKind.EventDeclaration,
                SyntaxKind.MethodDeclaration,
                SyntaxKind.ConstructorDeclaration,
                SyntaxKind.IndexerDeclaration,
                SyntaxKind.RecordDeclaration,
                SyntaxKind.RecordStructDeclaration,
                SyntaxKind.EnumMemberDeclaration,
                // Add other member types if needed
            };

#pragma warning disable IDE0060 // Remove unused parameter
        /// <summary>
        /// Asynchronously adds XML documentation to a given document based on a provided diagnostic, using GPT for generating the documentation.
        /// </summary>
        /// <param name="document">The document to which XML documentation should be added.</param>
        /// <param name="diagnostic">The diagnostic information used to generate the XML documentation.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A Task returning a Document with the new XML documentation added.</returns>
        public static async Task<Document> AddXmlDocumentationAsync(Document document, Location location, CancellationToken cancellationToken)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            OpenAIClient client = DocGptOptions.Instance.GetClient();

            ChatCompletionsOptions completionOptions = new ChatCompletionsOptions();
            if (!string.IsNullOrWhiteSpace(DocGptOptions.Instance.ModelDeploymentName))
            {
                completionOptions.DeploymentName = DocGptOptions.Instance.ModelDeploymentName;
            }

            // Find the node at the diagnostic span
            Microsoft.CodeAnalysis.Text.TextSpan diagnosticSpan = location.SourceSpan;
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxNode node = root.FindNode(diagnosticSpan);

            if (node != null)
            {
                if (HasOverrideModifier(node))
                {
                    return await DecorateWithInheritDocAsync(node, document, cancellationToken);
                }

                if (IsConstantLiteral(ref node))
                {
                    return await DecorateWithValueAsSummaryAsync(node as FieldDeclarationSyntax, document, cancellationToken);
                }

                // Get the body of the method
                string code = node.GetText().ToString();

                completionOptions.Messages.Add(new ChatRequestUserMessage($@"You are to take the C# code below and create a valid XML Documentation summary block for it according to .NET specifications. Use the following steps to determine what you compute for the answer:

1. If the given code is not a complete C# type or member declaration, stop computing and return nothing.
2. If you're not able to discern the purpose of the code with reasonable certainty, just return `/// <summary />`

```csharp
{code}
```

You are to give back only the XML documentation wrapped in a code block (```), do not respond with any other text."));

                try
                {
                    Azure.Response<ChatCompletions> completion = await client.GetChatCompletionsAsync(completionOptions, cancellationToken);
                    string comment = completion.Value.Choices[0].Message.Content;
                    ExtractXmlDocComment(ref comment);

                    SyntaxTriviaList commentTrivia = SyntaxFactory.ParseLeadingTrivia(comment).InsertRange(0, node.GetLeadingTrivia());
                    // Add the comment to the start of the node found by the analyzer
                    SyntaxNode newRoot = root.ReplaceNode(node, node.WithLeadingTrivia(commentTrivia /*.Insert(0, SyntaxFactory.CarriageReturnLineFeed).Add(SyntaxFactory.CarriageReturnLineFeed)*/));

                    // return a document with the new syntax root
                    document = document.WithSyntaxRoot(Formatter.Format(newRoot, document.Project.Solution.Workspace));
                }
                catch (Exception e)
                {
#if DEBUG
                    if (!(e is TaskCanceledException) && !(e is OperationCanceledException))
                    {
                        System.Diagnostics.Debugger.Break();
                    }
#else
                    throw;
#endif
                }
            }

            return document;
        }

        /// <summary>
        /// Extracts an XML comment from a given string, trimming any markdown code block symbols or language specifiers.
        /// </summary>
        /// <param name="comment">The string containing the XML comment to be extracted. This string is modified by this method.</param>
        private static void ExtractXmlDocComment(ref string comment)
        {
            // if the comment is surrounded by code block markdown, remove it and any language specifier
            int codeBlockLocation = comment.IndexOf("```", StringComparison.Ordinal);
            if (codeBlockLocation >= 0)
            {
                comment = comment.Substring(codeBlockLocation + 3);

                int idx = comment.IndexOf('\n');
                if (idx > 0)
                {
                    comment = comment.Substring(idx + 1);
                }
            }

            comment = $@"{comment.TrimStart().TrimEnd().TrimEnd('`', '\n', '\r')}
";
        }
    }
}
