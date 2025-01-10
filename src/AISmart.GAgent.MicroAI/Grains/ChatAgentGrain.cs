using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AISmart.Agent;
using AISmart.Agent.GEvents;
using AISmart.Options;
using AutoGen.Core;
using AutoGen.OpenAI;
using AutoGen.OpenAI.Extension;
using AutoGen.SemanticKernel;
using AutoGen.SemanticKernel.Extension;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.Google;
using OpenAI.Chat;
using Orleans;
using Orleans.Providers;

namespace AISmart.Grains;

[StorageProvider(ProviderName = "PubSubStore")]
public class ChatAgentGrain : Grain, IChatAgentGrain
{
    private MiddlewareStreamingAgent<SemanticKernelAgent>? _agent;
    private readonly MicroAIOptions _options;
    private readonly AIModelOptions _aiModelOptions;
    private readonly ILogger<ChatAgentGrain> _logger;

    public ChatAgentGrain(IOptions<MicroAIOptions> options, IOptions<AIModelOptions> aiModelOptions,
        ILogger<ChatAgentGrain> logger)
    {
        _options = options.Value;
        _aiModelOptions = aiModelOptions.Value;
        _logger = logger;
    }

    public async Task<MicroAIMessage?> SendAsync(string message, List<MicroAIMessage>? chatHistory)
    {
        if (_agent != null)
        {
            var history = ConvertMessage(chatHistory);
            var imMessage = await _agent.SendAsync(message, history);
            return new MicroAIMessage("assistant", imMessage.GetContent()!);
        }

        _logger.LogWarning($"[ChatAgentGrain] Agent is not set");
        return null;
    }

    public Task SetAgentAsync(string systemMessage)
    {
        var kernelBuilder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(_options.Model, _options.Endpoint, _options.ApiKey);
        var systemName = this.GetPrimaryKeyString();
        var kernel = kernelBuilder.Build();
        var kernelAgent = new SemanticKernelAgent(
                kernel: kernel,
                name: systemName,
                systemMessage: systemMessage)
            .RegisterMessageConnector();

        _agent = kernelAgent;
        return Task.CompletedTask;
    }


    public Task SetAgentWithRandomLLMAsync(string systemMessage)
    {
        string llm = GetRandomLlmType();
        return SetAgentAsync(systemMessage, llm);
    }

    public Task SetAgentAsync(string systemMessage, string llm)
    {
        var kernelBuilder = Kernel.CreateBuilder();

        ConfigureKernelBuilder(kernelBuilder, llm, _aiModelOptions);


        var kernel = kernelBuilder.Build();
        var systemName = this.GetPrimaryKeyString();
        var kernelAgent = new SemanticKernelAgent(
                kernel: kernel,
                name: systemName,
                systemMessage: systemMessage)
            .RegisterMessageConnector();

        _agent = kernelAgent;
        return Task.CompletedTask;
    }

    public Task SetAgentWithTemperature(string systemMessage, float temperature, int? seed = null,
        int? maxTokens = null)
    {
        var kernelBuilder = Kernel.CreateBuilder()
            .AddAzureOpenAIChatCompletion(_options.Model, _options.Endpoint, _options.ApiKey);
        var systemName = this.GetPrimaryKeyString();
        var kernel = kernelBuilder.Build();
        var kernelAgent = new SemanticKernelAgent(
                kernel: kernel,
                name: systemName,
                systemMessage: systemMessage)
            .RegisterMessageConnector();

        _agent = kernelAgent;
        return Task.CompletedTask;
    }

    private List<IMessage> ConvertMessage(List<MicroAIMessage> listAutoGenMessage)
    {
        var result = new List<IMessage>();
        foreach (var item in listAutoGenMessage)
        {
            result.Add(new TextMessage(GetRole(item.Role), item.Content));
        }

        return result;
    }

    private Role GetRole(string roleName)
    {
        switch (roleName)
        {
            case "user":
                return Role.User;
            case "assistant":
                return Role.Assistant;
            case "system":
                return Role.System;
            case "function":
                return Role.Function;
            default:
                return Role.User;
        }
    }

    private static string GetRandomLlmType()
    {
        var random = new Random();
        return LLMTypesConstant.AllSupportedLlmTypes[random.Next(LLMTypesConstant.AllSupportedLlmTypes.Count)];
    }

    private void ConfigureKernelBuilder(IKernelBuilder kernelBuilder, string llm, AIModelOptions aiModelOptions)
    {
        switch (llm)
        {
            case LLMTypesConstant.AzureOpenAI:
            {
                // Fetch Azure-specific configuration.
                var azureOptions = aiModelOptions.AzureOpenAI;

                // Add Azure OpenAI Chat Completion to the KernelBuilder.
                kernelBuilder.AddAzureOpenAIChatCompletion(
                    _options.Model, _options.Endpoint, _options.ApiKey);
                break;
            }

            case LLMTypesConstant.Bedrock:
            {
                // Fetch Bedrock-specific configuration.
                var bedrockOptions = aiModelOptions.Bedrock;

                #pragma warning disable SKEXP0070

                // Add Bedrock Chat Completion to the KernelBuilder.
                kernelBuilder.AddBedrockChatCompletionService(
                    modelId: bedrockOptions.Model,
                    serviceId: bedrockOptions.ServiceId
                );
                #pragma warning restore SKEXP0070

                break;
            }

            case LLMTypesConstant.GoogleGemini:
            {
                // Fetch Google Gemini-specific configuration.
                var googleOptions = aiModelOptions.GoogleGemini;

                #pragma warning disable SKEXP0070
                // Add Google Gemini Chat Completion to the KernelBuilder.
                kernelBuilder.AddGoogleAIGeminiChatCompletion(
                    modelId: googleOptions.Model,
                    apiKey: googleOptions.ApiKey,
                    apiVersion: GoogleAIVersion.V1, // Optional: API version.
                    serviceId: googleOptions.ServiceId // Optional: Target a specific service.
                );
                #pragma warning restore SKEXP0070

                break;
            }

            default:
                throw new ArgumentException($"Unsupported LLM type: {llm}");
        }
    }
}