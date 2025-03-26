using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Detects keywords in AI responses and triggers corresponding events
/// Used to connect AI responses to game systems via keyword detection
/// </summary>
[Serializable]
public class AIEventTrigger
{
    /// <summary>
    /// Represents a keyword rule for event triggering
    /// </summary>
    [Serializable]
    public class KeywordRule
    {
        [Tooltip("The keyword or phrase to look for in AI responses")]
        public string keyword;

        [Tooltip("Whether to use exact case matching")]
        public bool caseSensitive = false;

        [Tooltip("Event to trigger when keyword is found")]
        public UnityEvent<string> onKeywordDetected;

        /// <summary>
        /// Checks if the response contains this keyword
        /// </summary>
        /// <param name="response">AI response text</param>
        /// <returns>True if keyword is found in response</returns>
        public bool CheckForKeyword(string response)
        {
            if (string.IsNullOrEmpty(response) || string.IsNullOrEmpty(keyword))
                return false;

            // Perform case-sensitive or case-insensitive check
            if (caseSensitive)
            {
                return response.Contains(keyword);
            }
            else
            {
                return response.ToLower().Contains(keyword.ToLower());
            }
        }
    }

    // List of keyword rules to check
    [SerializeField] private List<KeywordRule> _keywordRules = new List<KeywordRule>();

    // Event fired for any keyword detection with keyword info
    [SerializeField] private UnityEvent<string, string> _onAnyKeywordDetected;

    // Public event for code subscribers
    public event Action<string, string> OnAnyKeywordDetected;

    /// <summary>
    /// Process the AI response to check for keywords and trigger events
    /// </summary>
    /// <param name="response">AI response text</param>
    /// <returns>Number of keywords detected</returns>
    public int ProcessResponse(string response)
    {
        if (string.IsNullOrEmpty(response))
            return 0;

        int keywordsDetected = 0;

        // Check each keyword rule
        foreach (var rule in _keywordRules)
        {
            if (rule.CheckForKeyword(response))
            {
                // Trigger rule-specific event
                rule.onKeywordDetected?.Invoke(response);

                // Trigger general event
                _onAnyKeywordDetected?.Invoke(rule.keyword, response);
                OnAnyKeywordDetected?.Invoke(rule.keyword, response);

                keywordsDetected++;

                // Log for debugging
                Debug.Log($"AIEventTrigger: Detected keyword '{rule.keyword}' in response");
            }
        }

        return keywordsDetected;
    }

    /// <summary>
    /// Add a new keyword rule at runtime
    /// </summary>
    /// <param name="keyword">Keyword to detect</param>
    /// <param name="caseSensitive">Whether to use case-sensitive matching</param>
    /// <param name="callback">Action to call when keyword is detected</param>
    public void AddKeywordRule(string keyword, bool caseSensitive, UnityAction<string> callback)
    {
        if (string.IsNullOrEmpty(keyword))
            return;

        // Create new rule
        var newRule = new KeywordRule
        {
            keyword = keyword,
            caseSensitive = caseSensitive
        };

        // Add callback
        if (callback != null)
        {
            newRule.onKeywordDetected = new UnityEvent<string>();
            newRule.onKeywordDetected.AddListener(callback);
        }

        // Add to list
        _keywordRules.Add(newRule);
    }

    /// <summary>
    /// Remove a keyword rule by its keyword
    /// </summary>
    /// <param name="keyword">Keyword to remove</param>
    /// <param name="caseSensitive">Whether to use case-sensitive matching for removal</param>
    /// <returns>True if rule was found and removed</returns>
    public bool RemoveKeywordRule(string keyword, bool caseSensitive = false)
    {
        if (string.IsNullOrEmpty(keyword))
            return false;

        for (int i = 0; i < _keywordRules.Count; i++)
        {
            if (caseSensitive)
            {
                if (_keywordRules[i].keyword == keyword)
                {
                    _keywordRules.RemoveAt(i);
                    return true;
                }
            }
            else
            {
                if (_keywordRules[i].keyword.ToLower() == keyword.ToLower())
                {
                    _keywordRules.RemoveAt(i);
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Get count of registered keyword rules
    /// </summary>
    public int RuleCount => _keywordRules.Count;

    /// <summary>
    /// Get a copy of all keyword rules
    /// </summary>
    /// <returns>List of keyword rules</returns>
    public List<KeywordRule> GetRules()
    {
        return new List<KeywordRule>(_keywordRules);
    }

    /// <summary>
    /// Clear all keyword rules
    /// </summary>
    public void ClearRules()
    {
        _keywordRules.Clear();
    }
}