namespace DocGpt
{
    using System;
    using System.Collections.Immutable;
    using System.Composition;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;

    using Azure.AI.OpenAI;

    using DocGpt.Options;

    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CodeActions;
    using Microsoft.CodeAnalysis.CodeFixes;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.Formatting;

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

            var code = root.GetText().GetSubText(diagnosticSpan).ToString();

            // Register a code action that will invoke the fix.
            context.RegisterCodeFix(CodeAction.Create(
                title: CodeFixResources.CodeFixTitle,
                createChangedDocument: c => AddXmlDocumentationViaGptAsync(context.Document, diagnostic, c),
                equivalenceKey: nameof(CodeFixResources.CodeFixTitle)),
            diagnostic);
        }

#pragma warning disable IDE0060 // Remove unused parameter
        private async Task<Document> AddXmlDocumentationViaGptAsync(Document document, Diagnostic diagnostic, CancellationToken cancellationToken)
#pragma warning restore IDE0060 // Remove unused parameter
        {
            var client = DocGptOptions.Instance.GetClient();

            var completionOptions = new ChatCompletionsOptions();
            if (!string.IsNullOrWhiteSpace(DocGptOptions.Instance.ModelDeploymentName))
            {
                completionOptions.DeploymentName = DocGptOptions.Instance.ModelDeploymentName;
            }

            // Find the node at the diagnostic span
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var root = await document.GetSyntaxRootAsync(cancellationToken);
            var node = root.FindNode(diagnosticSpan);

            if (node != null)
            {
                // Get the body of the method
                var code = node.GetText().ToString();

                completionOptions.Messages.Add(new ChatRequestUserMessage($@"You are to take the C# code below and create a valid XML Documentation summary block for it according to .NET specifications. Use the following steps to determine what you compute for the answer:

1. If the given code is not a complete C# type or member declaration, stop computing and return nothing.
2. If the member is an `override` or an implementation of an interface member, stop computing and return `/// <inheritdoc />`
3. If you're not able to discern the purpose of the code with reasonable certainty, return `/// <summary />`

```csharp
{code}
```

You are to give back only the XML documentation wrapped in a code block (```), do not respond with any other text."));

                var syntaxRoot = await document.GetSyntaxRootAsync();
                var diagNode = syntaxRoot.FindNode(diagnostic.Location.SourceSpan);
                try
                {
                    var completion = await client.GetChatCompletionsAsync(completionOptions, cancellationToken);
                    var comment = completion.Value.Choices[0].Message.Content;
                    ExtractXmlDocComment(ref comment);

                    var commentTrivia = SyntaxFactory.ParseLeadingTrivia(comment).InsertRange(0, diagNode.GetLeadingTrivia());
                    // Add the comment to the start of the node found by the analyzer
                    var newRoot = syntaxRoot.ReplaceNode(diagNode, diagNode.WithLeadingTrivia(commentTrivia /*.Insert(0, SyntaxFactory.CarriageReturnLineFeed).Add(SyntaxFactory.CarriageReturnLineFeed)*/));

                    // return a document with the new syntax root
                    document = document.WithSyntaxRoot(Formatter.Format(newRoot, document.Project.Solution.Workspace));
                }
                catch (Exception)
                {
                    //Debugger.Break();
                }
            }

            return document;
        }

        private void ExtractXmlDocComment(ref string comment)
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
