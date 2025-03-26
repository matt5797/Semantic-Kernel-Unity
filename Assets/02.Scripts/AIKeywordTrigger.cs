using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects keywords in AI responses and triggers events
/// Connects AIAgent responses to game systems via keyword detection
/// </summary>
[RequireComponent(typeof(AIAgent))]
public class AIKeywordTrigger : MonoBehaviour
{
    // Reference to the AIAgent component
    private AIAgent _aiAgent;

    // List of keyword triggers with associated events
    [Serializable]
    public class KeywordTrigger
    {
        public string keyword;
        public bool caseSensitive = false;
        public UnityEvent<string> onTriggered;

        [HideInInspector]
        public bool wasTriggered = false;

        public bool Matches(string text)
        {
            if (string.IsNullOrEmpty(keyword) || string.IsNullOrEmpty(text))
                return false;

            if (caseSensitive)
                return text.Contains(keyword);
            else
                return text.ToLower().Contains(keyword.ToLower());
        }
    }

    // List of keyword triggers configurable in Inspector
    [SerializeField]
    private List<KeywordTrigger> _keywordTriggers = new List<KeywordTrigger>();

    // Event raised when any keyword is detected
    [SerializeField]
    private UnityEvent<string, string> _onAnyKeywordDetected;

    // Whether to auto-reset triggers after a response
    [SerializeField]
    private bool _autoResetTriggers = true;

    /// <summary>
    /// Initialize and connect to AIAgent events
    /// </summary>
    private void Awake()
    {
        _aiAgent = GetComponent<AIAgent>();

        if (_aiAgent == null)
        {
            Debug.LogError($"AIKeywordTrigger ({gameObject.name}): AIAgent component not found!");
            enabled = false;
            return;
        }
    }

    /// <summary>
    /// Subscribe to AIAgent events
    /// </summary>
    private void OnEnable()
    {
        if (_aiAgent != null)
        {
            _aiAgent.OnResponseReceived += CheckKeywords;
        }
    }

    /// <summary>
    /// Unsubscribe from AIAgent events
    /// </summary>
    private void OnDisable()
    {
        if (_aiAgent != null)
        {
            _aiAgent.OnResponseReceived -= CheckKeywords;
        }
    }

    /// <summary>
    /// Check AI response for keywords and trigger events
    /// </summary>
    /// <param name="response">AI response text</param>
    private void CheckKeywords(string response)
    {
        if (string.IsNullOrEmpty(response) || _keywordTriggers == null)
            return;

        bool anyTriggered = false;
        string triggeredKeyword = string.Empty;

        // Check each keyword trigger
        foreach (var trigger in _keywordTriggers)
        {
            if (trigger.Matches(response))
            {
                // Mark as triggered
                trigger.wasTriggered = true;
                anyTriggered = true;
                triggeredKeyword = trigger.keyword;

                // Invoke the specific keyword event
                trigger.onTriggered?.Invoke(response);

                // Log for debugging
                Debug.Log($"AIKeywordTrigger: Keyword '{trigger.keyword}' detected in response");
            }
        }

        // Trigger general event if any keyword was found
        if (anyTriggered)
        {
            _onAnyKeywordDetected?.Invoke(triggeredKeyword, response);
        }

        // Auto-reset if enabled
        if (_autoResetTriggers)
        {
            ResetTriggers();
        }
    }

    /// <summary>
    /// Reset all keyword triggers
    /// </summary>
    public void ResetTriggers()
    {
        if (_keywordTriggers == null)
            return;

        foreach (var trigger in _keywordTriggers)
        {
            trigger.wasTriggered = false;
        }
    }

    /// <summary>
    /// Add a new keyword trigger programmatically
    /// </summary>
    /// <param name="keyword">Keyword to detect</param>
    /// <param name="caseSensitive">Whether detection is case sensitive</param>
    /// <param name="callback">Action to perform when triggered</param>
    /// <returns>Created KeywordTrigger instance</returns>
    public KeywordTrigger AddKeywordTrigger(string keyword, bool caseSensitive, UnityAction<string> callback)
    {
        if (string.IsNullOrEmpty(keyword))
            return null;

        var newTrigger = new KeywordTrigger
        {
            keyword = keyword,
            caseSensitive = caseSensitive,
            onTriggered = new UnityEvent<string>()
        };

        if (callback != null)
        {
            newTrigger.onTriggered.AddListener(callback);
        }

        _keywordTriggers.Add(newTrigger);
        return newTrigger;
    }

    /// <summary>
    /// Remove a keyword trigger
    /// </summary>
    /// <param name="keyword">Keyword to remove</param>
    /// <returns>True if successful</returns>
    public bool RemoveKeywordTrigger(string keyword)
    {
        if (string.IsNullOrEmpty(keyword) || _keywordTriggers == null)
            return false;

        return _keywordTriggers.RemoveAll(t => t.keyword == keyword) > 0;
    }

    /// <summary>
    /// Check if a specific keyword was triggered in the last response
    /// </summary>
    /// <param name="keyword">Keyword to check</param>
    /// <returns>True if triggered</returns>
    public bool WasKeywordTriggered(string keyword)
    {
        if (string.IsNullOrEmpty(keyword) || _keywordTriggers == null)
            return false;

        foreach (var trigger in _keywordTriggers)
        {
            if (trigger.keyword == keyword && trigger.wasTriggered)
                return true;
        }

        return false;
    }
}