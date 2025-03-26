using UnityEngine;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using Microsoft.SemanticKernel;

/// <summary>
/// Core component that bridges Unity with Semantic Kernel
/// Handles initialization, API key management, and request processing
/// Converts async calls to Unity coroutines
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

    // Events for kernel operations
    public event Action OnInitialized;
    public event Action<Exception> OnInitializationFailed;
    public event Action<string> OnResponseReceived;
    public event Action<Exception> OnRequestFailed;

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

    /// <summary>
    /// Initialize with default provider
    /// </summary>
    public void Initialize()
    {
        Initialize(LLMProvider.OpenAI);
    }

    /// <summary>
    /// Initialize with specific provider
    /// </summary>
    /// <param name="provider">LLM provider to use</param>
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

    /// <summary>
    /// Get API key for the specified provider
    /// </summary>
    /// <param name="provider">LLM provider</param>
    /// <returns>API key as string</returns>
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

    /// <summary>
    /// Initialize kernel with specified provider and API key
    /// </summary>
    /// <param name="provider">LLM provider to use</param>
    /// <param name="apiKey">API key for authentication</param>
    private void InitializeKernel(LLMProvider provider, string apiKey)
    {
        try
        {
            _kernel = CreateKernelForProvider(provider, apiKey);
            _isInitialized = (_kernel != null);

            if (_isInitialized)
            {
                Debug.Log($"SKUnityCore: Successfully initialized with {provider}");
                OnInitialized?.Invoke();
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"SKUnityCore: Initialization failed - {ex.Message}");
            Debug.LogException(ex);
            _isInitialized = false;
            OnInitializationFailed?.Invoke(ex);
        }
    }

    /// <summary>
    /// Create kernel based on provider
    /// </summary>
    /// <param name="provider">LLM provider</param>
    /// <param name="apiKey">API key</param>
    /// <returns>Configured Kernel instance</returns>
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

    /// <summary>
    /// Configure OpenAI kernel
    /// </summary>
    /// <param name="builder">Kernel builder</param>
    /// <param name="apiKey">API key</param>
    /// <returns>Configured Kernel</returns>
    private Kernel ConfigureOpenAIKernel(IKernelBuilder builder, string apiKey)
    {
        builder.AddOpenAIChatCompletion(
            modelId: "gpt-3.5-turbo",
            apiKey: apiKey
        );
        return builder.Build();
    }

    /// <summary>
    /// Configure Azure OpenAI kernel
    /// </summary>
    /// <param name="builder">Kernel builder</param>
    /// <param name="apiKey">API key</param>
    /// <returns>Configured Kernel</returns>
    private Kernel ConfigureAzureOpenAIKernel(IKernelBuilder builder, string apiKey)
    {
        builder.AddAzureOpenAIChatCompletion(
            deploymentName: "gpt-35-turbo",
            endpoint: _config.AzureOpenAIEndpoint,
            apiKey: apiKey
        );
        return builder.Build();
    }

    /// <summary>
    /// Configure Anthropic kernel
    /// </summary>
    /// <param name="builder">Kernel builder</param>
    /// <param name="apiKey">API key</param>
    /// <returns>Configured Kernel</returns>
    private Kernel ConfigureAnthropicKernel(IKernelBuilder builder, string apiKey)
    {
        // Placeholder as Semantic Kernel may not directly support Anthropic
        Debug.LogWarning("Anthropic provider not fully implemented yet");
        return builder.Build();
    }

    /// <summary>
    /// Execute prompt asynchronously using Tasks
    /// </summary>
    /// <param name="prompt">Prompt text</param>
    /// <returns>LLM response as string</returns>
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
            string response = result.GetValue<string>() ?? string.Empty;
            OnResponseReceived?.Invoke(response);
            return response;
        }
        catch (Exception ex)
        {
            Debug.LogError($"SKUnityCore: Execution failed - {ex.Message}");
            Debug.LogException(ex);
            OnRequestFailed?.Invoke(ex);
            return $"Error: {ex.Message}";
        }
    }

    // Request state for tracking and cancellation
    private class RequestState
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public CancellationTokenSource CancellationSource { get; } = new CancellationTokenSource();
        public bool IsCompleted { get; set; } = false;
        public float StartTime { get; } = Time.time;
    }

    // Dictionary of active requests for tracking and cancellation
    private Dictionary<string, RequestState> _activeRequests = new Dictionary<string, RequestState>();

    // Default timeout in seconds
    [SerializeField] private float _defaultTimeoutSeconds = 30f;

    // Optional queue component for request throttling
    [SerializeField] private SKRequestQueue _requestQueue;

    /// <summary>
    /// Execute prompt as Unity coroutine with timeout and cancellation support
    /// </summary>
    /// <param name="prompt">Prompt text</param>
    /// <param name="onComplete">Callback with response text</param>
    /// <param name="onError">Callback with error message</param>
    /// <param name="timeoutSeconds">Timeout duration in seconds (0 for no timeout)</param>
    /// <param name="useQueue">Whether to use request queue if available</param>
    /// <returns>Request ID that can be used for cancellation</returns>
    public string ExecutePrompt(string prompt, Action<string> onComplete, Action<string> onError = null, float timeoutSeconds = 0, bool useQueue = true)
    {
        RequestState requestState = new RequestState();
        _activeRequests[requestState.Id] = requestState;

        float actualTimeout = timeoutSeconds > 0 ? timeoutSeconds : _defaultTimeoutSeconds;

        // Use request queue if available and requested
        if (_requestQueue != null && useQueue)
        {
            _requestQueue.EnqueueRequest(
                () => StartCoroutine(ExecutePromptCoroutine(prompt, requestState, onComplete, onError, actualTimeout)),
                onComplete,
                onError
            );
        }
        else
        {
            StartCoroutine(ExecutePromptCoroutine(prompt, requestState, onComplete, onError, actualTimeout));
        }

        return requestState.Id;
    }

    /// <summary>
    /// Cancel a specific request by ID
    /// </summary>
    /// <param name="requestId">Request ID to cancel</param>
    /// <returns>True if request was found and cancelled</returns>
    public bool CancelRequest(string requestId)
    {
        if (string.IsNullOrEmpty(requestId) || !_activeRequests.TryGetValue(requestId, out RequestState state))
        {
            return false;
        }

        if (!state.IsCompleted)
        {
            state.CancellationSource.Cancel();
            _activeRequests.Remove(requestId);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Coroutine implementation for executing prompts with timeout and cancellation
    /// </summary>
    /// <param name="prompt">Prompt text</param>
    /// <param name="requestState">Request state for tracking</param>
    /// <param name="onComplete">Callback with response text</param>
    /// <param name="onError">Callback with error message</param>
    /// <param name="timeoutSeconds">Timeout duration in seconds</param>
    private IEnumerator ExecutePromptCoroutine(
        string prompt,
        RequestState requestState,
        Action<string> onComplete,
        Action<string> onError,
        float timeoutSeconds)
    {
        if (!_isInitialized || _kernel == null)
        {
            string errorMsg = "Error: System not initialized";
            Debug.LogError("SKUnityCore: " + errorMsg);
            onError?.Invoke(errorMsg);
            CleanupRequest(requestState.Id);
            yield break;
        }

        // Use TaskCompletionSource to bridge between Task and Coroutine
        var taskCompletionSource = new TaskCompletionSource<string>();

        // Execute the async task with cancellation token
        Task.Run(async () => {
            try
            {
                var function = _kernel.CreateFunctionFromPrompt(prompt);
                var result = await _kernel.InvokeAsync(function, cancellationToken: requestState.CancellationSource.Token);
                string response = result.GetValue<string>() ?? string.Empty;
                taskCompletionSource.SetResult(response);
            }
            catch (OperationCanceledException)
            {
                taskCompletionSource.SetCanceled();
            }
            catch (Exception ex)
            {
                taskCompletionSource.SetException(ex);
            }
        });

        // Wait for completion, cancellation, or timeout
        float startTime = Time.time;

        while (!taskCompletionSource.Task.IsCompleted)
        {
            // Check for timeout
            if (timeoutSeconds > 0 && Time.time - startTime > timeoutSeconds)
            {
                requestState.CancellationSource.Cancel();
                string timeoutMessage = $"Request timed out after {timeoutSeconds} seconds";
                Debug.LogWarning($"SKUnityCore: {timeoutMessage}");
                onError?.Invoke(timeoutMessage);
                CleanupRequest(requestState.Id);
                yield break;
            }

            yield return null;
        }

        // Handle the result
        if (taskCompletionSource.Task.IsCanceled)
        {
            string cancelMsg = "Request was cancelled";
            Debug.Log($"SKUnityCore: {cancelMsg}");
            onError?.Invoke(cancelMsg);
        }
        else if (taskCompletionSource.Task.IsFaulted)
        {
            Exception ex = taskCompletionSource.Task.Exception;
            Debug.LogError($"SKUnityCore: Execution failed - {ex.Message}");
            Debug.LogException(ex);
            OnRequestFailed?.Invoke(ex);
            onError?.Invoke($"Error: {ex.Message}");
        }
        else
        {
            string response = taskCompletionSource.Task.Result;
            OnResponseReceived?.Invoke(response);
            onComplete?.Invoke(response);
        }

        CleanupRequest(requestState.Id);
    }

    /// <summary>
    /// Clean up request resources and remove from tracking
    /// </summary>
    /// <param name="requestId">Request ID to clean up</param>
    private void CleanupRequest(string requestId)
    {
        if (_activeRequests.TryGetValue(requestId, out RequestState state))
        {
            state.IsCompleted = true;
            state.CancellationSource.Dispose();
            _activeRequests.Remove(requestId);
        }
    }

    /// <summary>
    /// Set API key for provider
    /// </summary>
    /// <param name="provider">LLM provider</param>
    /// <param name="apiKey">API key</param>
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

    /// <summary>
    /// Test API connection as coroutine with timeout
    /// </summary>
    /// <param name="provider">LLM provider to test</param>
    /// <param name="onComplete">Callback with success result</param>
    /// <param name="timeoutSeconds">Timeout in seconds (0 for default)</param>
    /// <param name="useQueue">Whether to use request queue if available</param>
    /// <returns>Request ID that can be used for cancellation</returns>
    public string TestConnection(LLMProvider provider, Action<bool> onComplete, float timeoutSeconds = 0, bool useQueue = true)
    {
        RequestState requestState = new RequestState();
        _activeRequests[requestState.Id] = requestState;

        float actualTimeout = timeoutSeconds > 0 ? timeoutSeconds : _defaultTimeoutSeconds;

        // Use request queue if available and requested
        if (_requestQueue != null && useQueue)
        {
            _requestQueue.EnqueueRequest(
                () => StartCoroutine(TestConnectionCoroutine(provider, requestState, onComplete, actualTimeout)),
                (result) => { /* No string result for this type */ },
                (error) => onComplete?.Invoke(false)
            );
        }
        else
        {
            StartCoroutine(TestConnectionCoroutine(provider, requestState, onComplete, actualTimeout));
        }

        return requestState.Id;
    }

    /// <summary>
    /// Coroutine implementation for testing connection with timeout
    /// </summary>
    /// <param name="provider">LLM provider to test</param>
    /// <param name="requestState">Request state for tracking</param>
    /// <param name="onComplete">Callback with success result</param>
    /// <param name="timeoutSeconds">Timeout in seconds</param>
    private IEnumerator TestConnectionCoroutine(
        LLMProvider provider,
        RequestState requestState,
        Action<bool> onComplete,
        float timeoutSeconds)
    {
        // Create temporary kernel for testing
        string apiKey = GetApiKeyForProvider(provider);
        if (string.IsNullOrEmpty(apiKey))
        {
            onComplete?.Invoke(false);
            CleanupRequest(requestState.Id);
            yield break;
        }

        // Use TaskCompletionSource to bridge between Task and Coroutine
        var taskCompletionSource = new TaskCompletionSource<bool>();

        // Execute the test asynchronously with cancellation
        Task.Run(async () => {
            try
            {
                var testKernel = CreateKernelForProvider(provider, apiKey);
                if (testKernel == null)
                {
                    taskCompletionSource.SetResult(false);
                    return;
                }

                var function = testKernel.CreateFunctionFromPrompt("Say hello");
                var result = await testKernel.InvokeAsync(function, cancellationToken: requestState.CancellationSource.Token);
                bool success = !string.IsNullOrEmpty(result.GetValue<string>());
                taskCompletionSource.SetResult(success);
            }
            catch (OperationCanceledException)
            {
                taskCompletionSource.SetCanceled();
            }
            catch (Exception ex)
            {
                Debug.LogError($"API Test failed: {ex.Message}");
                Debug.LogException(ex);
                taskCompletionSource.SetResult(false);
            }
        });

        // Wait for completion, cancellation, or timeout
        float startTime = Time.time;

        while (!taskCompletionSource.Task.IsCompleted)
        {
            // Check for timeout
            if (timeoutSeconds > 0 && Time.time - startTime > timeoutSeconds)
            {
                requestState.CancellationSource.Cancel();
                Debug.LogWarning($"SKUnityCore: Connection test timed out after {timeoutSeconds} seconds");
                onComplete?.Invoke(false);
                CleanupRequest(requestState.Id);
                yield break;
            }

            yield return null;
        }

        // Handle result
        if (taskCompletionSource.Task.IsCanceled)
        {
            Debug.Log("SKUnityCore: Connection test was cancelled");
            onComplete?.Invoke(false);
        }
        else
        {
            onComplete?.Invoke(taskCompletionSource.Task.Result);
        }

        CleanupRequest(requestState.Id);
    }

    /// <summary>
    /// Test API connection asynchronously using Tasks
    /// </summary>
    /// <param name="provider">LLM provider to test</param>
    /// <returns>True if connection successful</returns>
    public async Task<bool> TestConnectionAsync(LLMProvider provider)
    {
        // Create temporary kernel for testing
        string apiKey = GetApiKeyForProvider(provider);
        if (string.IsNullOrEmpty(apiKey)) return false;

        try
        {
            var testKernel = CreateKernelForProvider(provider, apiKey);
            if (testKernel == null) return false;

            // Create and run a simple test function
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