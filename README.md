# Doc GPT

A Visual Studio extension to quickly and easily document your code using LLMs.

## Installation

- Install the extension **free** from [the Visual Studio Marketplace](https://marketplace.visualstudio.com/items?itemName=bc3tech.net-bc3tech-docgpt) or from within Visual Studio's extension manager
- Set up the endpoint for your OpenAI model in the extension's settings
  ![VS Options panel for Doc GPT](docs/img/options_panel.png)

> Note: The model name is only used in the case of Azure OpenAI deployments. If you're using an account on OpenAI.org, you can leave this blank.

## Usage

The extension ships with two main components:

1. Analyzer which finds undocumented members
1. Code fix which generates documentation for the member

The analyzer details can be found in the [Shipped](DocGpt.CodeFixes/AnalyzerReleases.Shipped.md)/[Unshipped](DocGpt.CodeFixes/AnalyzerReleases.Unshipped.md) documentation.

The code fix also reacts to the built-in XML Documentation diagnostic ([CS1591](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs1591))

![DEMO](docs/img/demo.gif) ([demo video](docs/img/demo.mp4))

## Notes

Sending code to GPT can *very* quickly run into token throttling based on endpoint/account configurations. Additionally, please be conscious of the fact that you are very often charged **per token** sent to the API. Sending large code files to the API can quickly run up a large bill.

Using the code fixer in a "fix all" scenario results in numerous back-to-back calls to the GPT endpoint. This can request-based throttling. If you encounter this, please try again in a few minutes.

## FAQ

❓ I don't see the fixer option for a member. Why? \
The code fixer only shows if the options have been configured correctly. Make sure your endpoint URL is a valid full URL and that your API key is present.

❓ It's taking a super long time to generate documentation. Why? \
This will happen if you start to hit your throttling limits as the underlying client will respect the `Retry-After` header on the response and keep trying for a while. If you're using the Azure OpenAI deployment, you can try increasing the number of tokens per request in the Azure portal. Otherwise, you need to wait for your limits to reset.
