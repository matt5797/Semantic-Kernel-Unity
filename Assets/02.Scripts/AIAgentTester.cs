using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Simple test component for AIAgent demonstration
/// Provides basic UI controls and event handling for testing
/// </summary>
public class AIAgentTester : MonoBehaviour
{
    // References to required components
    [Header("Agent Setup")]
    [SerializeField] private SKUnityCore _semanticCore;
    [SerializeField] private AIAgent _aiAgent;

    [Header("UI Components")]
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _sendButton;
    [SerializeField] private Button _clearButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private TextMeshProUGUI _statusText;
    [SerializeField] private TextMeshProUGUI _outputText;
    [SerializeField] private ScrollRect _outputScrollRect;

    [Header("Keyword Testing")]
    [SerializeField] private string[] _testKeywords = { "hello", "goodbye", "help" };
    [SerializeField] private GameObject _keywordIndicator;
    [SerializeField] private TextMeshProUGUI _lastKeywordText;

    // Tracking variables
    private List<string> _conversationLog = new List<string>();
    private bool _isInitialized = false;

    /// <summary>
    /// Initialize the tester and connect event handlers
    /// </summary>
    private void Start()
    {
        // Validate components
        if (!ValidateComponents())
            return;

        // Set up UI events
        _sendButton.onClick.AddListener(SendMessage);
        _clearButton.onClick.AddListener(ClearConversation);
        _cancelButton.onClick.AddListener(CancelRequest);

        // Set up agent events
        _aiAgent.OnProcessingStarted += HandleProcessingStarted;
        _aiAgent.OnResponseReceived += HandleResponseReceived;
        _aiAgent.OnProcessingComplete += HandleProcessingComplete;
        _aiAgent.OnError += HandleError;
        _aiAgent.OnKeywordDetected += HandleKeywordDetected;

        // Set up test keywords
        SetupTestKeywords();

        // Update UI state
        UpdateUIState();

        // Mark as initialized
        _isInitialized = true;
        SetStatus("Ready to test. Type a message to begin.");
    }

    /// <summary>
    /// Clean up event handlers
    /// </summary>
    private void OnDestroy()
    {
        if (_aiAgent != null)
        {
            _aiAgent.OnProcessingStarted -= HandleProcessingStarted;
            _aiAgent.OnResponseReceived -= HandleResponseReceived;
            _aiAgent.OnProcessingComplete -= HandleProcessingComplete;
            _aiAgent.OnError -= HandleError;
            _aiAgent.OnKeywordDetected -= HandleKeywordDetected;
        }
    }

    /// <summary>
    /// Validate that all required components are assigned
    /// </summary>
    /// <returns>True if all components are valid</returns>
    private bool ValidateComponents()
    {
        bool isValid = true;

        if (_semanticCore == null)
        {
            Debug.LogError("AIAgentTester: SKUnityCore reference not set!");
            SetStatus("ERROR: SKUnityCore reference missing!");
            isValid = false;
        }

        if (_aiAgent == null)
        {
            Debug.LogError("AIAgentTester: AIAgent reference not set!");
            SetStatus("ERROR: AIAgent reference missing!");
            isValid = false;
        }

        if (_inputField == null || _sendButton == null || _outputText == null)
        {
            Debug.LogError("AIAgentTester: UI components not fully assigned!");
            SetStatus("ERROR: UI components missing!");
            isValid = false;
        }

        return isValid;
    }

    /// <summary>
    /// Set up keyword detection for testing
    /// </summary>
    private void SetupTestKeywords()
    {
        if (_aiAgent == null)
            return;

        foreach (string keyword in _testKeywords)
        {
            if (!string.IsNullOrEmpty(keyword))
            {
                _aiAgent.AddKeywordRule(keyword, false, OnKeywordTriggered);
                Debug.Log($"AIAgentTester: Added test keyword '{keyword}'");
            }
        }

        // Hide keyword indicator initially
        if (_keywordIndicator != null)
            _keywordIndicator.SetActive(false);
    }

    /// <summary>
    /// Send message to the AI agent
    /// </summary>
    public void SendMessage()
    {
        if (!_isInitialized || _aiAgent == null)
            return;

        string userInput = _inputField.text.Trim();
        if (string.IsNullOrEmpty(userInput))
            return;

        // Process input through agent
        _aiAgent.ProcessInput(userInput);

        // Add to conversation log
        AddToConversation("User: " + userInput);

        // Clear input field
        _inputField.text = "";

        // Update UI state
        UpdateUIState();
    }

    /// <summary>
    /// Clear the conversation history
    /// </summary>
    public void ClearConversation()
    {
        if (_aiAgent != null)
            _aiAgent.ClearChatHistory();

        _conversationLog.Clear();
        _outputText.text = "";

        // Reset keyword indicator
        if (_keywordIndicator != null)
            _keywordIndicator.SetActive(false);

        // Update status
        SetStatus("Conversation cleared.");

        // Update UI state
        UpdateUIState();
    }

    /// <summary>
    /// Cancel current AI request
    /// </summary>
    public void CancelRequest()
    {
        if (_aiAgent != null)
            _aiAgent.CancelRequest();

        // Update status
        SetStatus("Request cancelled.");

        // Update UI state
        UpdateUIState();
    }

    /// <summary>
    /// Handle when agent starts processing
    /// </summary>
    /// <param name="input">User input being processed</param>
    private void HandleProcessingStarted(string input)
    {
        SetStatus("Processing request...");
        UpdateUIState();
    }

    /// <summary>
    /// Handle when agent receives response
    /// </summary>
    /// <param name="response">AI response text</param>
    private void HandleResponseReceived(string response)
    {
        AddToConversation("AI: " + response);
    }

    /// <summary>
    /// Handle when agent completes processing
    /// </summary>
    /// <param name="response">Final response text</param>
    private void HandleProcessingComplete(string response)
    {
        SetStatus("Response received.");
        UpdateUIState();
    }

    /// <summary>
    /// Handle error from agent
    /// </summary>
    /// <param name="errorMessage">Error message</param>
    private void HandleError(string errorMessage)
    {
        SetStatus("ERROR: " + errorMessage);
        AddToConversation("System: Error occurred - " + errorMessage);
        UpdateUIState();
    }

    /// <summary>
    /// Handle keyword detection from agent
    /// </summary>
    /// <param name="keyword">Detected keyword</param>
    /// <param name="response">Full response text</param>
    private void HandleKeywordDetected(string keyword, string response)
    {
        Debug.Log($"AIAgentTester: Keyword detected: {keyword}");

        // Display keyword indicator
        if (_keywordIndicator != null)
            _keywordIndicator.SetActive(true);

        if (_lastKeywordText != null)
            _lastKeywordText.text = keyword;
    }

    /// <summary>
    /// Callback for keyword trigger events
    /// </summary>
    /// <param name="response">Response that triggered the keyword</param>
    private void OnKeywordTriggered(string response)
    {
        // This method receives direct keyword callbacks from the AIEventTrigger
        AddToConversation("System: Keyword detected in response!");
    }

    /// <summary>
    /// Add text to the conversation display
    /// </summary>
    /// <param name="text">Text to add</param>
    private void AddToConversation(string text)
    {
        _conversationLog.Add(text);
        UpdateConversationDisplay();
    }

    /// <summary>
    /// Update the conversation text display
    /// </summary>
    private void UpdateConversationDisplay()
    {
        _outputText.text = string.Join("\n\n", _conversationLog);

        // Scroll to bottom after update
        StartCoroutine(ScrollToBottom());
    }

    /// <summary>
    /// Scroll output to bottom (delayed for layout update)
    /// </summary>
    private IEnumerator ScrollToBottom()
    {
        // Wait for end of frame to ensure layout is updated
        yield return new WaitForEndOfFrame();

        if (_outputScrollRect != null)
            _outputScrollRect.verticalNormalizedPosition = 0f;
    }

    /// <summary>
    /// Set status text
    /// </summary>
    /// <param name="status">Status message</param>
    private void SetStatus(string status)
    {
        if (_statusText != null)
            _statusText.text = status;
    }

    /// <summary>
    /// Update UI elements based on current state
    /// </summary>
    private void UpdateUIState()
    {
        bool isProcessing = _aiAgent != null && _aiAgent.IsProcessing;

        // Update button states
        if (_sendButton != null)
            _sendButton.interactable = !isProcessing;

        if (_cancelButton != null)
            _cancelButton.interactable = isProcessing;

        if (_inputField != null)
            _inputField.interactable = !isProcessing;
    }
}