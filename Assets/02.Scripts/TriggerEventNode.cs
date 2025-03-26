using UnityEngine;
using System;
/*
/// <summary>
/// Node for Visual Scripting that triggers events based on AI response keywords
/// Can be connected to AIAgent to create game interactions based on AI responses
/// </summary>
public class TriggerEventNode : MonoBehaviour
{
    // Reference to the AI agent
    [SerializeField] private AIAgent _aiAgent;

    // Configuration
    [SerializeField] private string _keywordToDetect = "";
    [SerializeField] private bool _caseSensitive = false;

    // Event triggered when the keyword is detected
    public event Action<string> OnEventTriggered;

    // Unity events for Inspector connections
    [SerializeField] private UnityEngine.Events.UnityEvent<string> _onEventTriggered;

    // Optional event parameter override
    [SerializeField] private string _customEventParameter = "";

    // Debug options
    [SerializeField] private bool _debugMode = false;

    /// <summary>
    /// Connects to agent's events on start
    /// </summary>
    private void Start()
    {
        ValidateSetup();

        if (_aiAgent != null)
        {
            // Subscribe to keyword detection
            _aiAgent.OnKeywordDetected += HandleKeywordDetected;

            // Register this node's keyword with the agent
            AddKeywordToAgent();
        }
    }

    /// <summary>
    /// Unsubscribe from events when disabled
    /// </summary>
    private void OnDisable()
    {
        if (_aiAgent != null)
        {
            _aiAgent.OnKeywordDetected -= HandleKeywordDetected;
        }
    }

    /// <summary>
    /// Add this node's keyword to the agent's event trigger system
    /// </summary>
    private void AddKeywordToAgent()
    {
        if (string.IsNullOrEmpty(_keywordToDetect) || _aiAgent == null)
            return;

        _aiAgent.AddKeywordRule(
            _keywordToDetect,
            _caseSensitive,
            // This callback ensures Unity events work even with direct rule addition
            (response) => TriggerEvent(response)
        );

        if (_debugMode)
        {
            Debug.Log($"TriggerEventNode ({gameObject.name}): Registered keyword '{_keywordToDetect}' with agent");
        }
    }

    /// <summary>
    /// Handle detected keywords
    /// </summary>
    /// <param name="keyword">Detected keyword</param>
    /// <param name="response">Full response text</param>
    private void HandleKeywordDetected(string keyword, string response)
    {
        if (string.Equals(
            keyword,
            _keywordToDetect,
            _caseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase))
        {
            // Trigger the event
            TriggerEvent(response);
        }
    }

    /// <summary>
    /// Trigger the event and invoke callbacks
    /// </summary>
    /// <param name="response">Full response text</param>
    private void TriggerEvent(string response)
    {
        // Determine event parameter to use
        string eventParam = string.IsNullOrEmpty(_customEventParameter) ? response : _customEventParameter;

        // Invoke C# event
        OnEventTriggered?.Invoke(eventParam);

        // Invoke Unity event
        _onEventTriggered?.Invoke(eventParam);

        if (_debugMode)
        {
            Debug.Log($"TriggerEventNode ({gameObject.name}): Event triggered by keyword '{_keywordToDetect}'");
        }
    }

    /// <summary>
    /// Executes the node with a specific response
    /// Primarily used for direct invocation from Visual Scripting
    /// </summary>
    /// <param name="response">AI response to process</param>
    /// <returns>True if event was triggered</returns>
    public bool Execute(string response)
    {
        if (string.IsNullOrEmpty(response) || string.IsNullOrEmpty(_keywordToDetect))
            return false;

        // Check for keyword
        bool keywordFound = _caseSensitive
            ? response.Contains(_keywordToDetect)
            : response.ToLower().Contains(_keywordToDetect.ToLower());

        if (keywordFound)
        {
            TriggerEvent(response);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Update keyword at runtime
    /// </summary>
    /// <param name="newKeyword">New keyword to detect</param>
    /// <param name="caseSensitive">Whether to use case-sensitive matching</param>
    public void SetKeyword(string newKeyword, bool caseSensitive)
    {
        if (string.IsNullOrEmpty(newKeyword))
            return;

        // Remove old keyword
        if (_aiAgent != null && !string.IsNullOrEmpty(_keywordToDetect))
        {
            _aiAgent.RemoveKeywordRule(_keywordToDetect, _caseSensitive);
        }

        // Update properties
        _keywordToDetect = newKeyword;
        _caseSensitive = caseSensitive;

        // Add new keyword
        if (_aiAgent != null)
        {
            AddKeywordToAgent();
        }
    }

    /// <summary>
    /// Validate component setup and try to find agent if not set
    /// </summary>
    private void ValidateSetup()
    {
        if (_aiAgent == null)
        {
            // Try to find on this GameObject first
            _aiAgent = GetComponent<AIAgent>();

            // If not found, try to find in the scene
            if (_aiAgent == null)
            {
                _aiAgent = FindAnyObjectByType<AIAgent>();

                if (_aiAgent == null)
                {
                    Debug.LogWarning($"TriggerEventNode ({gameObject.name}): No AIAgent reference set or found");
                }
                else if (_debugMode)
                {
                    Debug.Log($"TriggerEventNode ({gameObject.name}): Found AIAgent in scene");
                }
            }
        }

        if (string.IsNullOrEmpty(_keywordToDetect))
        {
            Debug.LogWarning($"TriggerEventNode ({gameObject.name}): No keyword specified to detect");
        }
    }
}*/