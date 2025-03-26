using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Main component for AI-powered NPCs
/// Handles conversation processing and event triggering
/// Attaches to GameObject to add AI capabilities
/// </summary>
public class AIAgent : MonoBehaviour
{
    // Reference to the core Semantic Kernel bridge
    [SerializeField] private SKUnityCore _semanticCore;

    // Agent configuration
    [SerializeField] private string _agentName = "Assistant";
    [SerializeField] private string _agentDescription = "A helpful AI assistant.";
    [SerializeField, TextArea(3, 10)] private string _characterPromptTemplate = "You are {{character}}, {{description}}. Respond to: {{input}}";
    [SerializeField, Range(3, 10)] private int _maxHistorySize = 5;
    [SerializeField] private float _responseTimeout = 15f;

    // Internal components
    private ChatMemory _chatMemory;
    private PromptTemplate _promptTemplate;

    // Current request tracking
    private string _currentRequestId;
    private bool _isProcessingRequest = false;

    // Base events
    [SerializeField] private UnityEvent<string> _onResponseReceived;
    [SerializeField] private UnityEvent<string> _onProcessingStarted;
    [SerializeField] private UnityEvent<string> _onProcessingComplete;
    [SerializeField] private UnityEvent<string> _onError;

    // Event Trigger system
    [SerializeField] private AIEventTrigger _eventTrigger = new AIEventTrigger();

    // Public events for external subscribers
    public event Action<string> OnResponseReceived;
    public event Action<string> OnProcessingStarted;
    public event Action<string> OnProcessingComplete;
    public event Action<string> OnError;

    // Keyword detection event
    public event Action<string, string> OnKeywordDetected;

    /// <summary>
    /// Initialize agent components on Awake
    /// </summary>
    private void Awake()
    {
        InitializeComponents();
    }

    /// <summary>
    /// Set up required components and check dependencies
    /// </summary>
    private void InitializeComponents()
    {
        // Create chat memory
        _chatMemory = new ChatMemory(_maxHistorySize);

        // Create prompt template
        _promptTemplate = new PromptTemplate(_characterPromptTemplate);
        _promptTemplate.SetVariable("character", _agentName);
        _promptTemplate.SetVariable("description", _agentDescription);

        // Find semantic core if not assigned
        if (_semanticCore == null)
        {
            _semanticCore = FindAnyObjectByType<SKUnityCore>();
            if (_semanticCore == null)
            {
                Debug.LogError($"AIAgent ({gameObject.name}): No SKUnityCore found in scene");
            }
        }

        // Connect keyword detection event
        _eventTrigger.OnAnyKeywordDetected += (keyword, response) => OnKeywordDetected?.Invoke(keyword, response);
    }

    /// <summary>
    /// Process user input and generate AI response
    /// </summary>
    /// <param name="input">User input text</param>
    public void ProcessInput(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            Debug.LogWarning($"AIAgent ({gameObject.name}): Empty input received");
            return;
        }

        if (_isProcessingRequest)
        {
            Debug.LogWarning($"AIAgent ({gameObject.name}): Already processing a request");
            return;
        }

        if (_semanticCore == null || !_semanticCore.IsInitialized)
        {
            string errorMsg = "Semantic core not initialized";
            Debug.LogError($"AIAgent ({gameObject.name}): {errorMsg}");
            _onError?.Invoke(errorMsg);
            OnError?.Invoke(errorMsg);
            return;
        }

        // Start processing
        _isProcessingRequest = true;
        _onProcessingStarted?.Invoke(input);
        OnProcessingStarted?.Invoke(input);

        // Add user message to chat memory
        _chatMemory.AddMessage(ChatMemory.MessageRole.User, input);

        // Generate prompt with chat history
        string chatHistory = _chatMemory.GetFormattedHistory();
        _promptTemplate.SetVariable("input", input);
        _promptTemplate.SetVariable("history", chatHistory);
        string prompt = _promptTemplate.GetFormattedPrompt();

        // Send request to Semantic Kernel
        _currentRequestId = _semanticCore.ExecutePrompt(
            prompt,
            OnResponseReceived_Internal,
            OnError_Internal,
            _responseTimeout
        );
    }

    /// <summary>
    /// Handle response from the LLM
    /// </summary>
    /// <param name="response">Response text</param>
    private void OnResponseReceived_Internal(string response)
    {
        // Add assistant response to chat memory
        _chatMemory.AddMessage(ChatMemory.MessageRole.Assistant, response);

        // Trigger response events
        _onResponseReceived?.Invoke(response);
        OnResponseReceived?.Invoke(response);

        // Process event triggers based on keywords
        ProcessEventTriggers(response);

        // Trigger completion events
        _onProcessingComplete?.Invoke(response);
        OnProcessingComplete?.Invoke(response);

        // Reset request state
        _isProcessingRequest = false;
        _currentRequestId = null;
    }

    /// <summary>
    /// Process keyword-based event triggers
    /// </summary>
    /// <param name="response">Response text to check for keywords</param>
    private void ProcessEventTriggers(string response)
    {
        if (string.IsNullOrEmpty(response))
            return;

        // Process through event trigger system
        int keywordsDetected = _eventTrigger.ProcessResponse(response);

        if (keywordsDetected > 0)
        {
            Debug.Log($"AIAgent ({gameObject.name}): Detected {keywordsDetected} keywords in response");
        }
    }

    /// <summary>
    /// Handle errors during request processing
    /// </summary>
    /// <param name="errorMessage">Error description</param>
    private void OnError_Internal(string errorMessage)
    {
        Debug.LogError($"AIAgent ({gameObject.name}): {errorMessage}");

        // Trigger events
        _onError?.Invoke(errorMessage);
        _onProcessingComplete?.Invoke(string.Empty);
        OnError?.Invoke(errorMessage);
        OnProcessingComplete?.Invoke(string.Empty);

        // Reset request state
        _isProcessingRequest = false;
        _currentRequestId = null;
    }

    /// <summary>
    /// Cancel current request if it's in progress
    /// </summary>
    public void CancelRequest()
    {
        if (!_isProcessingRequest || string.IsNullOrEmpty(_currentRequestId))
        {
            return;
        }

        if (_semanticCore != null)
        {
            _semanticCore.CancelRequest(_currentRequestId);
            _isProcessingRequest = false;
            _currentRequestId = null;
        }
    }

    /// <summary>
    /// Clear chat history for this agent
    /// </summary>
    public void ClearChatHistory()
    {
        _chatMemory.Clear();
    }

    /// <summary>
    /// Add a keyword rule to the event trigger system
    /// </summary>
    /// <param name="keyword">Keyword to detect</param>
    /// <param name="caseSensitive">Whether to use case-sensitive matching</param>
    /// <param name="callback">Action to call when keyword is detected</param>
    public void AddKeywordRule(string keyword, bool caseSensitive, UnityAction<string> callback)
    {
        _eventTrigger.AddKeywordRule(keyword, caseSensitive, callback);
    }

    /// <summary>
    /// Remove a keyword rule from the event trigger system
    /// </summary>
    /// <param name="keyword">Keyword to remove</param>
    /// <param name="caseSensitive">Whether to use case-sensitive matching for removal</param>
    /// <returns>True if rule was found and removed</returns>
    public bool RemoveKeywordRule(string keyword, bool caseSensitive = false)
    {
        return _eventTrigger.RemoveKeywordRule(keyword, caseSensitive);
    }

    /// <summary>
    /// Check if agent is currently processing a request
    /// </summary>
    public bool IsProcessing => _isProcessingRequest;

    /// <summary>
    /// Get current chat messages
    /// </summary>
    public System.Collections.Generic.List<ChatMemory.ChatMessage> GetChatHistory()
    {
        return _chatMemory.GetMessages();
    }

    /// <summary>
    /// Update agent properties at runtime
    /// </summary>
    /// <param name="agentName">New agent name</param>
    /// <param name="description">New agent description</param>
    public void UpdateAgentProperties(string agentName, string description)
    {
        _agentName = agentName;
        _agentDescription = description;

        _promptTemplate.SetVariable("character", _agentName);
        _promptTemplate.SetVariable("description", _agentDescription);
    }

    /// <summary>
    /// Access to the event trigger system
    /// </summary>
    public AIEventTrigger EventTrigger => _eventTrigger;
}