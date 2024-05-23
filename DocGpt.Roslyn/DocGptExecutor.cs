namespace DocGpt
{
    using System;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Threading.Tasks;

    using Azure.AI.OpenAI;

    using DocGpt.Options;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;

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

        /// <summary>
        /// Asynchronously adds XML documentation to a given document based on a provided diagnostic, using GPT for generating the documentation.
        /// </summary>
        /// <param name="node">The node to add XML documentation to.</param>
        /// <param name="cancellationToken">A cancellation token for the operation.</param>
        /// <returns>A Task returning a Document with the new XML documentation added.</returns>
        public static async Task<(SyntaxNode newNode, SyntaxNode nodeToReplace)> AddXmlDocumentationAsync(SyntaxNode node, CancellationToken cancellationToken)
        {
            _ = node ?? throw new ArgumentNullException(nameof(node));

            if (HasOverrideModifier(node) && DocGptOptions.Instance.OverridesBehavior is OverrideBehavior.UseInheritDoc)
            {
                return (DecorateWithInheritDoc(node), node);
            }

            if (IsConstantLiteral(node, out Microsoft.CodeAnalysis.CSharp.Syntax.FieldDeclarationSyntax parentField) && DocGptOptions.Instance.UseValueForLiteralConstants is true)
            {
                return (DecorateWithValueAsSummary(parentField), parentField);
            }

            // Get the body of the method
            var code = node.GetText().ToString();

            try
            {
                OpenAIClient client = DocGptOptions.Instance.GetClient();

                var completionOptions = new ChatCompletionsOptions();
                if (!string.IsNullOrWhiteSpace(DocGptOptions.Instance.ModelDeploymentName))
                {
                    completionOptions.DeploymentName = DocGptOptions.Instance.ModelDeploymentName;
                }

                completionOptions.Messages.Add(new ChatRequestUserMessage($@"You are to take the C# code below and create a valid XML Documentation summary block for it according to .NET specifications. Use the following steps to determine what you compute for the answer:

1. If the given code is not a complete C# type or member declaration, stop computing and return nothing.
2. If the declaration is a variable or field, your summary should attempt to discern what the variable may mean based on its name and - if assigned - its value. Don't include the ""gets or sets"" verbiage.
3. If you're not able to discern the purpose of the code with reasonable certainty, just return `/// <summary />`

```csharp
{code}
```

You are to give back only the XML documentation wrapped in a code block (```), do not respond with any other text."));

                try
                {
                    Azure.Response<ChatCompletions> completion = await client.GetChatCompletionsAsync(completionOptions, cancellationToken);
                    var comment = completion.Value.Choices[0].Message.Content;
                    ExtractXmlDocComment(ref comment);

                    SyntaxTriviaList commentTrivia = CreateTrivia(node, comment);
                    // Add the comment to the start of the node found by the analyzer
                    return (node.WithLeadingTrivia(commentTrivia), node);
                }
                catch (Exception e)
                {
                    if (!(e is TaskCanceledException) && !(e is OperationCanceledException))
                    {
                        System.Diagnostics.Debugger.Break();
                    }

                    throw;
                }
            }
            catch (ArgumentNullException e)
            {
                return (node.WithLeadingTrivia(SyntaxFactory.Comment($"// Missing {e.ParamName} - Make sure you've entered the necessary information in Tools | Options | Doc GPT and try again.\r\n")), node);
            }
        }

        private static readonly Regex _lineFeedRegex = new Regex("(?<!\r)\n", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        private static readonly Regex _crlfRegex = new Regex("\r\n", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Singleline | RegexOptions.CultureInvariant);

        private static SyntaxTriviaList CreateTrivia(SyntaxNode node, string comment)
        {
            // If the node's line endings are all crlf, make sure GPT's generated output ends in crlf as well
            var nodeCurrentText = node.GetText().ToString();
            var numLineFeeds = _lineFeedRegex.Matches(nodeCurrentText).Count;
            var numCrLfs = _crlfRegex.Matches(nodeCurrentText).Count;

            if (numLineFeeds > 0)
            {
                if (numCrLfs is 0)
                {
                    // if the node has only line feeds, make sure the comment ends in line feeds
                    comment = _crlfRegex.Replace(comment, "\n");
                }

                // If the node already has mixed endings, don't do anything
            }
            else if (numCrLfs > 0)
            {
                // if the node has only crlfs, make sure the comment ends in crlfs
                comment = _lineFeedRegex.Replace(comment, "\r\n");
            }

            return SyntaxFactory.ParseLeadingTrivia(comment).InsertRange(0, node.GetLeadingTrivia());
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "Readability")]
        internal static bool NodeTriggersGpt(SyntaxNode node)
        {
            if (HasOverrideModifier(node))
            {
                return DocGptOptions.Instance.OverridesBehavior is OverrideBehavior.GptSummarize;
            }

            if (IsConstantLiteral(node, out _))
            {
                return !(DocGptOptions.Instance.UseValueForLiteralConstants is true);
            }

            return true;
        }

        /// <summary>
        /// Extracts an XML comment from a given string, trimming any markdown code block symbols or language specifiers.
        /// </summary>
        /// <param name="comment">The string containing the XML comment to be extracted. This string is modified by this method.</param>
        private static void ExtractXmlDocComment(ref string comment)
        {
            // if the comment is surrounded by code block markdown, remove it and any language specifier
            var codeBlockLocation = comment.IndexOf("```", StringComparison.Ordinal);
            if (codeBlockLocation >= 0)
            {
                comment = comment.Substring(codeBlockLocation + 3);

                var idx = comment.IndexOf('\n');
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
