using UnityEngine;
using System;
using System.Threading.Tasks;
using Microsoft.SemanticKernel;

/// <summary>
/// Core component that bridges Unity with Semantic Kernel
/// Handles initialization, API key management, and request processing
/// </summary>
public class SKUnityCore : MonoBehaviour
{
    [SerializeField] private SKUnityCoreConfig _config;
    private Kernel _kernel;
    private bool _isInitialized = false;

    // Supported LLM providers
    public enum LLMProvider
    {
        OpenAI,
        AzureOpenAI,
        Anthropic
    }

    private void Awake()
    {
        // Auto-initialize if config is available
        if (_config != null)
        {
            Initialize(LLMProvider.OpenAI);
        }
        else
        {
            Debug.LogError("SKUnityCore: Config not assigned!");
        }
    }

    // Initialize with default provider
    public void Initialize()
    {
        Initialize(LLMProvider.OpenAI);
    }

    // Initialize with specific provider
    public void Initialize(LLMProvider provider)
    {
        string apiKey = GetApiKeyForProvider(provider);
        if (string.IsNullOrEmpty(apiKey))
        {
            Debug.LogError($"SKUnityCore: API Key not set for {provider}");
            return;
        }

        InitializeKernel(provider, apiKey);
    }

    // Get API key based on provider
    private string GetApiKeyForProvider(LLMProvider provider)
    {
        switch (provider)
        {
            case LLMProvider.OpenAI:
                return _config.OpenAIKey;
            case LLMProvider.AzureOpenAI:
                return _config.AzureOpenAIKey;
            case LLMProvider.Anthropic:
                return _config.AnthropicKey;
            default:
                return string.Empty;
        }
    }

    // Create kernel for specified provider
    private void InitializeKernel(LLMProvider provider, string apiKey)
    {
        try
        {
            _kernel = CreateKernelForProvider(provider, apiKey);
            _isInitialized = (_kernel != null);

            if (_isInitialized)
            {
                Debug.Log($"SKUnityCore: Successfully initialized with {provider}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"SKUnityCore: Initialization failed - {ex.Message}");
            Debug.LogException(ex);
            _isInitialized = false;
        }
    }

    // Create appropriate kernel based on provider
    private Kernel CreateKernelForProvider(LLMProvider provider, string apiKey)
    {
        var builder = Kernel.CreateBuilder();

        switch (provider)
        {
            case LLMProvider.OpenAI:
                return ConfigureOpenAIKernel(builder, apiKey);

            case LLMProvider.AzureOpenAI:
                return ConfigureAzureOpenAIKernel(builder, apiKey);

            case LLMProvider.Anthropic:
                return ConfigureAnthropicKernel(builder, apiKey);

            default:
                return builder.Build();
        }
    }

    // Configure OpenAI kernel
    private Kernel ConfigureOpenAIKernel(IKernelBuilder builder, string apiKey)
    {
        builder.AddOpenAIChatCompletion(
            modelId: "gpt-3.5-turbo",
            apiKey: apiKey
        );
        return builder.Build();
    }

    // Configure Azure OpenAI kernel
    private Kernel ConfigureAzureOpenAIKernel(IKernelBuilder builder, string apiKey)
    {
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: "gpt-35-turbo",
            endpoint: _config.AzureOpenAIEndpoint,
            apiKey: apiKey
        );
        return builder.Build();
    }

    // Configure Anthropic kernel (placeholder)
    private Kernel ConfigureAnthropicKernel(IKernelBuilder builder, string apiKey)
    {
        // Placeholder as Semantic Kernel may not directly support Anthropic
        Debug.LogWarning("Anthropic provider not fully implemented yet");
        return builder.Build();
    }

    // Execute prompt asynchronously
    public async Task<string> ExecutePromptAsync(string prompt)
    {
        if (!_isInitialized || _kernel == null)
        {
            Debug.LogError("SKUnityCore: Not initialized!");
            return "Error: System not initialized";
        }

        try
        {
            // Simple string prompt approach for latest SK versions
            var function = _kernel.CreateFunctionFromPrompt(prompt);
            var result = await _kernel.InvokeAsync(function);
            return result.GetValue<string>() ?? string.Empty;
        }
        catch (Exception ex)
        {
            Debug.LogError($"SKUnityCore: Execution failed - {ex.Message}");
            Debug.LogException(ex);
            return $"Error: {ex.Message}";
        }
    }

    // Set API key for provider
    public void SetApiKey(LLMProvider provider, string apiKey)
    {
        switch (provider)
        {
            case LLMProvider.OpenAI:
                _config.OpenAIKey = apiKey;
                break;
            case LLMProvider.AzureOpenAI:
                _config.AzureOpenAIKey = apiKey;
                break;
            case LLMProvider.Anthropic:
                _config.AnthropicKey = apiKey;
                break;
        }

        // Re-initialize if already initialized
        if (_isInitialized)
        {
            Initialize(provider);
        }
    }

    // Test API connection
    public async Task<bool> TestConnectionAsync(LLMProvider provider)
    {
        // Create temporary kernel for testing
        string apiKey = GetApiKeyForProvider(provider);
        if (string.IsNullOrEmpty(apiKey)) return false;

        try
        {
            var testKernel = CreateKernelForProvider(provider, apiKey);
            if (testKernel == null) return false;

            // Simple string prompt approach for latest SK versions
            var function = testKernel.CreateFunctionFromPrompt("Say hello");
            var result = await testKernel.InvokeAsync(function);

            return !string.IsNullOrEmpty(result.GetValue<string>());
        }
        catch (Exception ex)
        {
            Debug.LogError($"API Test failed: {ex.Message}");
            Debug.LogException(ex);
            return false;
        }
    }
}